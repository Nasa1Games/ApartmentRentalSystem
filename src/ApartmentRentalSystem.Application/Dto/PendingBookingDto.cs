namespace ApartmentRentalSystem.Application.Dto;

public record PendingBookingDto(
    Guid BookingId,
    string TenantFullName,
    string ApartmentUnitNumber,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    decimal TotalPrice,
    string Status
);