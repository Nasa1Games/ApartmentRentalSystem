using ApartmentRentalSystem.Core.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Bookings;

// Команда -- контейнер данных, поэтому должна быть иммутабельной.
// IRequest<bool> означает что команда вернет true при успехе и false при ошибке
public record CompleteBookingCommand(Guid BookingId) : IRequest<bool>;


public class CompleteBookingCommandHandler : IRequestHandler<CompleteBookingCommand, bool>
{
    // Репозиторий + управление подключением
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    // Метод выполнения заявки -- группирует логику выполненной заявки воедино
    public async Task<bool> Handle(CompleteBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Начинаем транзакцию
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 2. Получаем сущность
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return false;
            }

            // 3. Вызываем доменный метод (генерирует BookingCompletedEvent)
            booking.Complete();

            // 4. Сохраняем через репозиторий
            await _bookingRepository.UpdateAsync(booking);

            // 5. Коммит (UnitOfWork сохранит данные и опубликует событие)
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