using ApartmentRentalSystem.Application.DTOs;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.Specifications;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Application.Services;

public class ApartmentService : IApartmentService
{
    private readonly IApartmentRepository _repository;

    public ApartmentService(IApartmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ApartmentListItemDto>> GetAvailableApartmentsAsync(CancellationToken ct = default)
    {
        var criteria = new ApartmentFilterCriteria 
        { 
            Status = ApartmentStatus.Available.ToString() 
        };
        
        var apartments = await _repository.GetFilteredAsync(criteria);

        return apartments.Select(a => new ApartmentListItemDto(
            Id: a.Id,
            UnitNumber: a.UnitNumber,
            Floor: a.Floor,
            Capacity: a.Capacity,
            Price: a.BasePricePerNight.Amount,
            Status: a.Status.ToString()
        ));
    }
}