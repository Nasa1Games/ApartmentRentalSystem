using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;

namespace ApartmentRentalSystem.Core.Events;

public record BookingCompletedEvent(
    Guid BookingId,
    Guid TenantId,
    Guid ApartmentId,
    DateTime CompletedAt,
    Money TotalPrice
) : INotification;