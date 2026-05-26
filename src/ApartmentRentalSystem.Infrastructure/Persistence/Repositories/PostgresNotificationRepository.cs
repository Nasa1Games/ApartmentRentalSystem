using System.Text;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Infrastructure.Persistence.Db;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Repositories;

public class PostgresNotificationRepository : INotificationRepository
{
    private readonly IDbConnectionFactory _factory;

    public PostgresNotificationRepository(IDbConnectionFactory factory) => _factory = factory;

    private static Notification MapToNotification(dynamic row)
    {
        var dict = (IDictionary<string, object>)row;

        T Get<T>(string key, T defaultValue = default)
        {
            if (!dict.TryGetValue(key, out var val) || val == null || val == DBNull.Value)
                return defaultValue;
            return (T)Convert.ChangeType(val, typeof(T));
        }

        string GetStr(string key) => Get<string>(key);
        string? GetNullableStr(string key)
        {
            if (!dict.TryGetValue(key, out var val) || val == null || val == DBNull.Value)
                return null;
            return val.ToString();
        }

        return new Notification(
            id: Get<Guid>("id"),
            userId: Get<Guid>("user_id"),
            type: Enum.Parse<NotificationType>(GetStr("type"), ignoreCase: true),
            channel: Enum.Parse<NotificationChannel>(GetStr("channel"), ignoreCase: true),
            subject: GetNullableStr("subject"),
            body: GetNullableStr("body"),
            status: Enum.Parse<NotificationStatus>(GetStr("status"), ignoreCase: true),
            createdAt: Get<DateTime>("created_at"));
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            user_id, 
            type, 
            channel, 
            subject, 
            body, 
            status, 
            created_at
        FROM notifications WHERE id = @Id";
        
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row == null ? null : MapToNotification(row);
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            user_id, 
            type, 
            channel, 
            subject, 
            body, 
            status, 
            created_at
        FROM notifications 
        WHERE user_id = @UserId 
        ORDER BY created_at DESC";
        
        var rows = await conn.QueryAsync(sql, new { UserId = userId });
        return rows.Select(MapToNotification);
    }

    public async Task<IEnumerable<Notification>> GetPendingByChannelAsync(NotificationChannel channel)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            user_id, 
            type, 
            channel, 
            subject, 
            body, 
            status, 
            created_at
        FROM notifications 
        WHERE status = 'Pending' AND channel = @Channel 
        ORDER BY created_at ASC";
        
        var rows = await conn.QueryAsync(sql, new { Channel = channel.ToString() });
        return rows.Select(MapToNotification);
    }

    public async Task<IEnumerable<Notification>> GetFilteredAsync(
        Guid? userId = null,
        NotificationStatus? status = null,
        NotificationType? type = null,
        NotificationChannel? channel = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        using var conn = _factory.CreateConnection();
        var sql = new StringBuilder(@"
        SELECT 
            id, 
            user_id, 
            type, 
            channel, 
            subject, 
            body, 
            status, 
            created_at
        FROM notifications 
        WHERE 1=1");

        var parameters = new DynamicParameters();
        if (userId.HasValue)
        {
            sql.Append(" AND user_id = @UserId");
            parameters.Add("UserId", userId.Value);
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
        if (channel.HasValue)
        {
            sql.Append(" AND channel = @Channel");
            parameters.Add("Channel", channel.Value.ToString());
        }
        if (fromDate.HasValue)
        {
            sql.Append(" AND created_at >= @FromDate");
            parameters.Add("FromDate", fromDate.Value);
        }
        if (toDate.HasValue)
        {
            sql.Append(" AND created_at <= @ToDate");
            parameters.Add("ToDate", toDate.Value);
        }

        sql.Append(" ORDER BY created_at DESC");

        var rows = await conn.QueryAsync(sql.ToString(), parameters);
        return rows.Select(MapToNotification);
    }

    public async Task AddAsync(Notification notification)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO notifications (id, user_id, type, channel, subject, body, status, created_at)
            VALUES (@Id, @UserId, @Type, @Channel, @Subject, @Body, @Status, @CreatedAt)";
        
        await conn.ExecuteAsync(sql, new
        {
            notification.Id,
            UserId = notification.UserId,
            Type = notification.Type.ToString(),
            Channel = notification.Channel.ToString(),
            notification.Subject,
            notification.Body,
            Status = notification.Status.ToString(),
            notification.CreatedAt
        });
    }

    public async Task UpdateStatusAsync(Guid id, NotificationStatus status)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            UPDATE notifications 
            SET status = @Status 
            WHERE id = @Id";
        
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Status = status.ToString()
        });
    }

    public async Task UpdateContentAsync(Guid id, string? subject, string? body)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            UPDATE notifications 
            SET subject = @Subject, body = @Body 
            WHERE id = @Id";
        
        await conn.ExecuteAsync(sql, new
        {
            Id = id,
            Subject = (object?)subject ?? DBNull.Value,
            Body = (object?)body ?? DBNull.Value
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM notifications WHERE id = @Id", new { Id = id });
    }
}