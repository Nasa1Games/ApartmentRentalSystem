using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Entities;

public class Notification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string? Subject { get; private set; }
    public string? Body { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Notification() { } // Для Dapper

    public Notification(
        Guid id,
        Guid userId,
        NotificationType type,
        NotificationChannel channel,
        string? subject,
        string? body,
        NotificationStatus status,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        Type = type;
        Channel = channel;
        Subject = subject;
        Body = body;
        Status = status;
        CreatedAt = createdAt;
    }

    public Notification(
        Guid userId,
        NotificationType type,
        NotificationChannel channel,
        string? subject = null,
        string? body = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Channel = channel;
        Subject = subject;
        Body = body;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException($"Можно отметить как отправленное только уведомление в статусе Pending. Текущий статус: {Status}");
        
        Status = NotificationStatus.Sent;
    }

    public void MarkAsFailed()
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException($"Можно отметить как неудачное только уведомление в статусе Pending. Текущий статус: {Status}");
        
        Status = NotificationStatus.Failed;
    }

    public void UpdateContent(string? subject, string? body)
    {
        if (Status != NotificationStatus.Pending)
            throw new InvalidOperationException("Можно изменить содержимое только уведомления в статусе Pending");
        
        Subject = subject;
        Body = body;
    }

    public bool IsDelivered() => Status == NotificationStatus.Sent;
}