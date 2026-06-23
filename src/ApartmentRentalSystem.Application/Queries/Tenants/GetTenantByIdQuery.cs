using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Tenants;

public record GetTenantByIdQuery(Guid TenantId) : IRequest<Tenant?>;

public class GetTenantByIdQueryHandler : IRequestHandler<GetTenantByIdQuery, Tenant?>
{
    private readonly ITenantRepository _repository;

    public GetTenantByIdQueryHandler(ITenantRepository repository) 
        => _repository = repository;

    public async Task<Tenant?> Handle(
        GetTenantByIdQuery request, CancellationToken cancellationToken) 
        => await _repository.GetByIdAsync(request.TenantId);
}