using System.Text;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.Specifications;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Infrastructure.Persistence.Db;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Repositories;

public class PostgresBookingRepository : IBookingRepository
{
    private readonly IDbConnectionFactory _factory;
    public PostgresBookingRepository(IDbConnectionFactory factory) => _factory = factory;

    private static Booking MapToBooking(dynamic row)
    {
        var dict = (IDictionary<string, object>)row;

        // 🔹 Универсальный безопасный getter
        T Get<T>(string key, T defaultValue = default)
        {
            if (!dict.TryGetValue(key, out var val) || val == null || val == DBNull.Value)
                return defaultValue;
            return (T)Convert.ChangeType(val, typeof(T));
        }

        // ✅ Исправлено: явно указан тип string
        string GetStr(string key) => Get<string>(key) ?? string.Empty;

        DateTime GetDt(string key) => Get<DateTime>(key);
        DateTime? GetDtNullable(string key)
        {
            if (!dict.TryGetValue(key, out var val) || val == null || val == DBNull.Value)
                return null;
            return (DateTime)Convert.ChangeType(val, typeof(DateTime));
        }

        return new Booking(
            id: Get<Guid>("id"),
            tenantId: Get<Guid>("tenant_id"),
            apartmentId: Get<Guid>("apartment_id"),
            period: new DateRange(GetDt("check_in_date"), GetDt("check_out_date")),
            totalPrice: new Money(Get<decimal>("total_price")),
            deposit: new Money(Get<decimal>("deposit")),
            status: Enum.Parse<BookingStatus>(GetStr("status"), ignoreCase: true),
            cancellationReason: GetStr("cancellation_reason"),
            createdAt: GetDt("created_at"),
            updatedAt: GetDtNullable("updated_at"));
    }
    
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            tenant_id, 
            apartment_id,
            check_in_date, 
            check_out_date,
            status, 
            total_price, 
            deposit,
            cancellation_reason, 
            created_at, 
            updated_at
        FROM bookings WHERE id = @Id";
        
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row == null ? null : MapToBooking(row);
    }
    
    public async Task<IEnumerable<Booking>> GetFilteredAsync(BookingFilterCriteria criteria)
    {
        using var conn = _factory.CreateConnection();
        var sql = new StringBuilder(@"
        SELECT 
            id, 
            tenant_id, 
            apartment_id,
            check_in_date, 
            check_out_date,
            status, 
            total_price, 
            deposit,
            cancellation_reason, 
            created_at, 
            updated_at
        FROM bookings WHERE 1=1");

        if (criteria.TenantId.HasValue)
            sql.Append(" AND tenant_id = @TenantId");
        if (!string.IsNullOrEmpty(criteria.Status))
            sql.Append(" AND status = @Status");
        if (criteria.FromDate.HasValue)
            sql.Append(" AND check_in_date >= @FromDate");
        if (criteria.ToDate.HasValue)
            sql.Append(" AND check_in_date <= @ToDate");

        sql.Append(" ORDER BY created_at DESC");

        var parameters = new DynamicParameters();
        if (criteria.TenantId.HasValue) parameters.Add("TenantId", criteria.TenantId);
        if (!string.IsNullOrEmpty(criteria.Status)) parameters.Add("Status", criteria.Status);
        if (criteria.FromDate.HasValue) parameters.Add("FromDate", criteria.FromDate);
        if (criteria.ToDate.HasValue) parameters.Add("ToDate", criteria.ToDate);

        var row = await conn.QueryAsync<Booking>(sql.ToString(), parameters);
        return row.Select(MapToBooking);
    }

    public async Task<IEnumerable<Booking>> GetPendingAsync()
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            tenant_id, 
            apartment_id,
            check_in_date, 
            check_out_date,
            status, 
            total_price, 
            deposit,
            cancellation_reason, 
            created_at, 
            updated_at
        FROM bookings WHERE status = 'Pending' ORDER BY created_at DESC";
        
        var row = await conn.QueryAsync(sql);
        return row.Select(MapToBooking);
    }

    public async Task AddAsync(Booking booking)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO bookings (id, tenant_id, apartment_id, check_in_date, check_out_date, status, total_price, deposit, created_at)
            VALUES (@Id, @TenantId, @ApartmentId, @CheckInDate, @CheckOutDate, @Status, @TotalPrice, @Deposit, @CreatedAt)";
        
        await conn.ExecuteAsync(sql, new
        {
            booking.Id, booking.TenantId, booking.ApartmentId,
            CheckInDate = booking.Period.Start.Date,
            CheckOutDate = booking.Period.End.Date,
            Status = booking.Status.ToString(),
            TotalPrice = booking.TotalPrice.Amount,
            Deposit = booking.Deposit.Amount,
            booking.CreatedAt
        });
    }

    public async Task UpdateStatusAsync(Guid id, BookingStatus status, string? reason = null)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            UPDATE bookings SET status = @Status, updated_at = @UpdatedAt, cancellation_reason = @Reason
            WHERE id = @Id";
        
        await conn.ExecuteAsync(sql, new 
        { 
            Id = id, 
            Status = status.ToString(), 
            UpdatedAt = DateTime.UtcNow, 
            Reason = reason 
        });
    }
    
    public async Task<IEnumerable<Booking>> GetByTenantIdAsync(Guid tenantId)
    {
        using var conn = _factory.CreateConnection();
        
        const string sql = @"
            SELECT 
                id, 
                tenant_id, 
                apartment_id,
                check_in_date, 
                check_out_date,
                status, 
                total_price, 
                deposit,
                cancellation_reason, 
                created_at, 
                updated_at
            FROM bookings 
            WHERE tenant_id = @TenantId 
            ORDER BY created_at DESC";
        
        var row = await conn.QueryAsync<Booking>(sql, new { TenantId = tenantId });
        return row.Select(MapToBooking);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM bookings WHERE id = @Id", new { Id = id });
    }
}