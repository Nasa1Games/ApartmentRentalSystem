using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Apartments;

public record UpdateApartmentCommand(
    Guid ApartmentId,
    string? UnitNumber = null,
    int? Entrance = null,
    int? Floor = null,
    int? Capacity = null,
    decimal? BasePricePerNight = null,
    string? Description = null
) : IRequest<bool>;

public class UpdateApartmentCommandHandler : IRequestHandler<UpdateApartmentCommand, bool>
{
    private readonly IApartmentRepository _apartmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateApartmentCommandHandler(
        IApartmentRepository apartmentRepository,
        IUnitOfWork unitOfWork)
    {
        _apartmentRepository = apartmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(UpdateApartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // Быстрая валидация
            if (request.Capacity.HasValue && request.Capacity <= 0)
                throw new ArgumentException("Вместимость должна быть больше 0");
            if (request.BasePricePerNight.HasValue && request.BasePricePerNight <= 0)
                throw new ArgumentException("Цена должна быть больше 0");

            var apartment = await _apartmentRepository.GetByIdAsync(request.ApartmentId);
            if (apartment == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return false;
            }
            
            // Обновляем только указанные поля
            apartment.Update(
                unitNumber: request.UnitNumber,
                entrance: request.Entrance,
                floor: request.Floor,
                capacity: request.Capacity,
                basePricePerNight: request.BasePricePerNight.HasValue 
                    ? new Money(request.BasePricePerNight.Value) 
                    : null,
                description: request.Description
            );

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