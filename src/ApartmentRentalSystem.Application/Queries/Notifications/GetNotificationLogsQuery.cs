using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace ApartmentRentalSystem.Application.Queries.Notifications;

public class NotificationLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public DateTime SentAt { get; set; }
}

public record GetNotificationLogsQuery : IRequest<List<NotificationLogDto>>;

public class GetNotificationLogsQueryHandler : IRequestHandler<GetNotificationLogsQuery, List<NotificationLogDto>>
{
    private readonly INotificationDbConnectionFactory _connectionFactory;

    public GetNotificationLogsQueryHandler(INotificationDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<NotificationLogDto>> Handle(GetNotificationLogsQuery request, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT 
                n.id,
                n.user_id,
                n.type,
                n.channel,
                u.email as recipient,
                n.subject,
                n.body,
                n.status,
                n.created_at as sent_at
            FROM notifications n
            LEFT JOIN users u ON n.user_id = u.id
            ORDER BY n.created_at DESC
            LIMIT 100";

        using var connection = _connectionFactory.CreateConnection();
        var notifications = await connection.QueryAsync<NotificationLogDto>(sql);
        
        return notifications.AsList();
    }
}