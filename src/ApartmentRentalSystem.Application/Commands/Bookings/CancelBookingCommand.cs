using MediatR;
using ApartmentRentalSystem.Core.Interfaces;


namespace ApartmentRentalSystem.Application.Commands.Bookings;

// Команда -- контейнер данных, поэтому должна быть иммутабельной.
// IRequest<bool> означает что команда вернет true при успехе и false при ошибке
public record CancelBookingCommand(Guid BookingId, string? Reason = null) : IRequest<bool>;


public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, bool>
{
    // Репозиторий + управление подключением
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    // Метод отклонения брони -- группирует логику отклонения брони воедино
    public async Task<bool> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Начинаем транзакцию
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Получаем сущность через репозиторий
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return false;
            }

            // 3. Вызываем доменный метод (генерирует BookingCancelledEvent)
            booking.Cancel(request.Reason ?? "Причина не указана");

            // 4. Сохраняем через репозиторий
            await _bookingRepository.UpdateAsync(booking);

            // 5. Коммит (UnitOfWork сохранит данные в БД и опубликует событие)
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