using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Tenants;

public record BlockTenantCommand(Guid TenantId) : IRequest<bool>;


public class BlockTenantCommandHandler : IRequestHandler<BlockTenantCommand, bool>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public async Task<bool> Handle(BlockTenantCommand request, CancellationToken cancellationToken)
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

            tenant.Block();
            await _tenantRepository.UpdateAsync(tenant);

            // - - Автоматически отменяем все PENDING брони - -
            var pendingBookings = 
                await _bookingRepository.GetByTenantIdAsync(tenant.Id);
            
            foreach (var booking in pendingBookings
                         .Where(b => b.Status == BookingStatus.Pending))
            {
                booking.Cancel("Аккаунт арендатора заблокирован");
                await _bookingRepository.UpdateAsync(booking);
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            return false;
        }
    }
}