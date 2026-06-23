using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Apartments;

public record ChangeApartmentStatusCommand(
    Guid ApartmentId,
    ApartmentStatus NewStatus
) : IRequest<bool>;


public class ChangeApartmentStatusCommandHandler : IRequestHandler<ChangeApartmentStatusCommand, bool>
{
    private readonly IApartmentRepository _apartmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeApartmentStatusCommandHandler(
        IApartmentRepository apartmentRepository,
        IUnitOfWork unitOfWork)
    {
        _apartmentRepository = apartmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ChangeApartmentStatusCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var apartment = await _apartmentRepository.GetByIdAsync(request.ApartmentId);
            if (apartment == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return false;
            }

            // Проверяем валидность перехода статуса
            if (request.NewStatus == ApartmentStatus.Occupied && 
                apartment.Status != ApartmentStatus.Available)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    "Можно установить статус Occupied только для доступных квартир");
            }

            // Вызываем доменный метод
            apartment.ChangeStatus(request.NewStatus);

            await _apartmentRepository.UpdateAsync(apartment);
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