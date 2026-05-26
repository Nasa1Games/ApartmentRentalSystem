using System.Text;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Infrastructure.Persistence.Db;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Repositories;

public class PostgresTransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _factory;

    public PostgresTransactionRepository(IDbConnectionFactory factory) => _factory = factory;

    private static Transaction MapToTransaction(dynamic row)
    {
        var dict = (IDictionary<string, object>)row;

        T Get<T>(string key, T defaultValue = default)
        {
            if (!dict.TryGetValue(key, out var val) || val == null || val == DBNull.Value)
                return defaultValue;
            return (T)Convert.ChangeType(val, typeof(T));
        }

        string GetStr(string key) => Get<string>(key) ?? string.Empty;
        DateTime? GetDtNullable(string key)
        {
            if (!dict.TryGetValue(key, out var val) || val == null || val == DBNull.Value)
                return null;
            return (DateTime)Convert.ChangeType(val, typeof(DateTime));
        }

        return new Transaction(
            id: Get<Guid>("id"),
            bookingId: Get<Guid>("booking_id"),
            amount: new Money(Get<decimal>("amount")),
            type: Enum.Parse<TransactionType>(GetStr("type"), ignoreCase: true),
            status: Enum.Parse<TransactionStatus>(GetStr("status"), ignoreCase: true),
            processedAt: GetDtNullable("processed_at"));
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            SELECT id, booking_id, amount, type, status, processed_at
            FROM transactions WHERE id = @Id";
        
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row == null ? null : MapToTransaction(row);
    }

    public async Task<IEnumerable<Transaction>> GetByBookingIdAsync(Guid bookingId)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            SELECT id, booking_id, amount, type, status, processed_at
            FROM transactions 
            WHERE booking_id = @BookingId 
            ORDER BY processed_at DESC NULLS LAST";
        
        var rows = await conn.QueryAsync(sql, new { BookingId = bookingId });
        return rows.Select(MapToTransaction);
    }

    public async Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            SELECT id, booking_id, amount, type, status, processed_at
            FROM transactions 
            WHERE status = @Status 
            ORDER BY processed_at DESC NULLS LAST";
        
        var rows = await conn.QueryAsync(sql, new { Status = status.ToString() });
        return rows.Select(MapToTransaction);
    }

    public async Task<IEnumerable<Transaction>> GetFilteredAsync(
        Guid? bookingId = null,
        TransactionStatus? status = null,
        TransactionType? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = new StringBuilder(@"
            SELECT id, booking_id, amount, type, status, processed_at
            FROM transactions WHERE 1=1");

        var parameters = new DynamicParameters();

        if (bookingId.HasValue)
        {
            sql.Append(" AND booking_id = @BookingId");
            parameters.Add("BookingId", bookingId.Value);
        }
        if (status.HasValue)
        {
            sql.Append(" AND status = @Status");
            parameters.Add("Status", status.Value.ToString());
        }
        if (type.HasValue)
        {
            sql.Append(" AND type = @Type");
            parameters.Add("Type", type.Value.ToString());
        }
        if (fromDate.HasValue)
        {
            sql.Append(" AND processed_at >= @FromDate");
            parameters.Add("FromDate", fromDate.Value);
        }
        if (toDate.HasValue)
        {
            sql.Append(" AND processed_at <= @ToDate");
            parameters.Add("ToDate", toDate.Value);
        }

        sql.Append(" ORDER BY processed_at DESC NULLS LAST");

        var rows = await conn.QueryAsync(sql.ToString(), parameters);
        return rows.Select(MapToTransaction);
    }

    public async Task AddAsync(Transaction transaction)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO transactions (id, booking_id, amount, type, status, processed_at)
            VALUES (@Id, @BookingId, @Amount, @Type, @Status, @ProcessedAt)";
        
        await conn.ExecuteAsync(sql, new
        {
            transaction.Id,
            BookingId = transaction.BookingId,
            Amount = transaction.Amount.Amount,
            Type = transaction.Type.ToString(),
            Status = transaction.Status.ToString(),
            ProcessedAt = transaction.ProcessedAt
        });
    }

    public async Task UpdateStatusAsync(Guid id, TransactionStatus status)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            UPDATE transactions 
            SET status = @Status, processed_at = @ProcessedAt
            WHERE id = @Id";
        
        var processedAt = status == TransactionStatus.Pending ? (DateTime?)null : DateTime.UtcNow;
        
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status.ToString(),
            ProcessedAt = processedAt
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM transactions WHERE id = @Id", new { Id = id });
    }
}