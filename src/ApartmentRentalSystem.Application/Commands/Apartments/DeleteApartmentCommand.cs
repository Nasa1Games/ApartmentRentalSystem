using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Apartments;

public record DeleteApartmentCommand(Guid ApartmentId) : IRequest<bool>;


public class DeleteApartmentCommandHandler : IRequestHandler<DeleteApartmentCommand, bool>
{
    private readonly IApartmentRepository _apartmentRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteApartmentCommandHandler(
        IApartmentRepository apartmentRepository,
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _apartmentRepository = apartmentRepository;
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(
        DeleteApartmentCommand request, CancellationToken cancellationToken)
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

            // Проверяем, нет ли активных броней
            var bookings = 
                await _bookingRepository.GetByApartmentIdAsync(apartment.Id);
            
            var activeBookings = bookings
                .Where(b => 
                    b.Status is BookingStatus.Pending or BookingStatus.Approved)
                .ToList();

            if (activeBookings.Count != 0)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Невозможно удалить квартиру: есть {activeBookings.Count} активных броней");
            }

            await _apartmentRepository.DeleteAsync(apartment.Id);
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