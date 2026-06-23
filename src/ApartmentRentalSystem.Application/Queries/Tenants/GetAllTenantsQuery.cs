using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Tenants;

public record GetAllTenantsQuery : IRequest<IEnumerable<Tenant>>;

public class GetAllTenantsQueryHandler : IRequestHandler<GetAllTenantsQuery, IEnumerable<Tenant>>
{
    private readonly ITenantRepository _repository;

    public GetAllTenantsQueryHandler(ITenantRepository repository)
        => _repository = repository;

    public async Task<IEnumerable<Tenant>> Handle(
        GetAllTenantsQuery request, CancellationToken cancellationToken) 
        => await _repository.GetAllAsync();
}