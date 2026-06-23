using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Apartments;

// 🔹 DTO для передачи данных квартиры
public record ApartmentDto(
    Guid Id,
    string UnitNumber,
    int Entrance,
    int Floor,
    int Capacity,
    decimal BasePricePerNight,
    string? Description,
    ApartmentStatus Status
);

// 🔹 Query для получения одной квартиры по ID
public record GetApartmentByIdQuery(Guid Id) : IRequest<ApartmentDto?>;

// 🔹 Обработчик
public class GetApartmentByIdQueryHandler : IRequestHandler<GetApartmentByIdQuery, ApartmentDto?>
{
    private readonly IApartmentRepository _repository;

    public GetApartmentByIdQueryHandler(IApartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApartmentDto?> Handle(GetApartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var apartment = await _repository.GetByIdAsync(request.Id);
        
        if (apartment == null)
            return null;

        return new ApartmentDto(
            apartment.Id,
            apartment.UnitNumber,
            apartment.Entrance,
            apartment.Floor,
            apartment.Capacity,
            apartment.BasePricePerNight.Amount,
            apartment.Description,
            apartment.Status
        );
    }
}