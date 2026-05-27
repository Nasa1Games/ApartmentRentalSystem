namespace ApartmentRentalSystem.Application.DTOs;

public record ApartmentListItemDto(
    Guid Id,
    string UnitNumber,
    int Floor,
    int Capacity,
    decimal Price,
    string Status);