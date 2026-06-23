using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Tenants;

public record LinkTelegramCommand(
    Guid TenantId,
    long TelegramId
) : IRequest<bool>;

public class LinkTelegramCommandHandler : IRequestHandler<LinkTelegramCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LinkTelegramCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(LinkTelegramCommand request, CancellationToken cancellationToken)
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

            // Проверяем, не привязан ли этот TelegramId к другому пользователю
            var existingTenant = await _tenantRepository.GetByTelegramIdAsync(request.TelegramId);
            if (existingTenant != null && existingTenant.Id != request.TenantId)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Этот Telegram аккаунт уже привязан к другому пользователю");
            }

            tenant.UpdateTelegramId(request.TelegramId);

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