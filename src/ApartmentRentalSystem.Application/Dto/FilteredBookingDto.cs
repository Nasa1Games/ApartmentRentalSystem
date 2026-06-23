namespace ApartmentRentalSystem.Application.Dto;

public record FilteredBookingDto(
    Guid BookingId,
    string TenantFullName,
    string ApartmentUnitNumber,
    int ApartmentEntrance,
    int ApartmentFloor,
    int ApartmentCapacity,
    decimal TotalPrice,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    string Status
);