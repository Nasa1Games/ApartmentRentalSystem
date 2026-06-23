using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;

namespace ApartmentRentalSystem.Core.Events;

/// <summary>
/// 
/// </summary>
public record BookingApprovedEvent(
    Guid BookingId,
    Guid TenantId,
    Guid ApartmentId,
    DateTime ApprovedAt,
    DateRange Period,
    Money TotalPrice
) : INotification;