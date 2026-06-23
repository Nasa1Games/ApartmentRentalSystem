using MediatR;

namespace ApartmentRentalSystem.Core.Events;

public record BookingRejectedEvent(
    Guid BookingId,
    Guid TenantId,
    Guid ApartmentId,
    string Reason,
    DateTime RejectedAt
) : INotification;