using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Tenants;

public record SetNotificationPreferenceCommand(
    Guid TenantId,
    NotificationChannel PreferredChannel
) : IRequest<bool>;


public class SetNotificationPreferenceCommandHandler : IRequestHandler<SetNotificationPreferenceCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetNotificationPreferenceCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(SetNotificationPreferenceCommand request, CancellationToken cancellationToken)
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

            tenant.SetNotificationPreference(request.PreferredChannel);

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