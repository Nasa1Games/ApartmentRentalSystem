using MediatR;

namespace ApartmentRentalSystem.Core.Events;

public record BookingCancelledEvent(
    Guid BookingId,
    Guid TenantId,
    Guid ApartmentId,
    string Reason,
    DateTime CancelledAt
) : INotification;