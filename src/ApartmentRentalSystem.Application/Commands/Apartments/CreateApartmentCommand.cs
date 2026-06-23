using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Apartments;

public record CreateApartmentCommand(
    string UnitNumber,
    int Entrance,
    int Floor,
    int Capacity,
    decimal BasePricePerNight,
    string? Description = null
) : IRequest<Guid>;


public class CreateApartmentCommandHandler : IRequestHandler<CreateApartmentCommand, Guid>
{
    private readonly IApartmentRepository _apartmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateApartmentCommandHandler(
        IApartmentRepository apartmentRepository,
        IUnitOfWork unitOfWork)
    {
        _apartmentRepository = apartmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateApartmentCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Валидация
            if (request.Capacity <= 0)
                throw new ArgumentException("Вместимость должна быть больше 0");
            if (request.BasePricePerNight <= 0)
                throw new ArgumentException("Цена должна быть больше 0");
            if (request.Entrance <= 0)
                throw new ArgumentException("Номер подъезда должен быть больше 0");
            if (request.Floor <= 0)
                throw new ArgumentException("Номер этажа должен быть больше 0");

            // Создаем сущность
            var apartment = new Apartment(
                unitNumber: request.UnitNumber,
                entrance: request.Entrance,
                floor: request.Floor,
                capacity: request.Capacity,
                basePricePerNight: new Money(request.BasePricePerNight),
                description: request.Description
            );

            await _apartmentRepository.AddAsync(apartment);
            await _unitOfWork.CommitAsync(cancellationToken);

            return apartment.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}