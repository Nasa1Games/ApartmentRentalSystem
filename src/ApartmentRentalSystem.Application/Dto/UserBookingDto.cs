namespace ApartmentRentalSystem.Application.Dto;

public record UserBookingDto(
    Guid BookingId,
    string ApartmentUnitNumber,
    int ApartmentEntrance,
    int ApartmentFloor,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    string Status,
    decimal TotalPrice
);