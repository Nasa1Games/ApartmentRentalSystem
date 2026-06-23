namespace ApartmentRentalSystem.Application.Dto;

public record BookingDetailsDto(
    // Информация о брони
    Guid BookingId,
    Guid TenantId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    decimal TotalPrice,
    decimal Deposit,
    string Status,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,

    // Информация о клиенте
    string TenantFullName,
    string TenantEmail,
    string TenantPhone,
    long? TenantTelegramId,

    // Информация о квартире
    string ApartmentUnitNumber,
    int ApartmentEntrance,
    int ApartmentFloor,
    int ApartmentCapacity,
    decimal ApartmentPricePerNight
);