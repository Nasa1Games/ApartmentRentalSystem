using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Tenants;

public record LinkEmailCommand(
    Guid TenantId,
    string Email
) : IRequest<bool>;


public class LinkEmailCommandHandler : IRequestHandler<LinkEmailCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LinkEmailCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LinkEmailCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return false;
            }

            // Проверяем, не занят ли email другим пользователем
            var existingTenant = await _tenantRepository.GetByEmailAsync(request.Email);
            if (existingTenant != null && existingTenant.Id != request.TenantId)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Email {request.Email} уже занят другим пользователем");
            }

            tenant.UpdateEmail(request.Email);

            await _tenantRepository.UpdateAsync(tenant);
            await _unitOfWork.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return false;
        }
    }
}