using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Notification>> GetPendingByChannelAsync(NotificationChannel channel);
    Task<IEnumerable<Notification>> GetFilteredAsync(
        Guid? userId = null,
        NotificationStatus? status = null,
        NotificationType? type = null,
        NotificationChannel? channel = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);
    
    Task AddAsync(Notification notification);
    Task UpdateStatusAsync(Guid id, NotificationStatus status);
    Task UpdateContentAsync(Guid id, string? subject, string? body);
    Task DeleteAsync(Guid id);
}