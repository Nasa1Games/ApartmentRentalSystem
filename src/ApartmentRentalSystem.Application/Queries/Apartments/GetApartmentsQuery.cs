using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Apartments;

// Запрос принимает критерии фильтрации из Core
public record GetApartmentsQuery(ApartmentFilterCriteria Criteria) : IRequest<IEnumerable<Apartment>>;


public class GetApartmentsQueryHandler : IRequestHandler<GetApartmentsQuery, IEnumerable<Apartment>>
{
    private readonly IApartmentRepository _repository;

    public GetApartmentsQueryHandler(IApartmentRepository repository) 
        => _repository = repository;

    public async Task<IEnumerable<Apartment>> Handle(
        GetApartmentsQuery request, CancellationToken cancellationToken) 
        => await _repository.GetFilteredAsync(request.Criteria);
}