using MediatR;
using ApartmentRentalSystem.Core.Interfaces;


namespace ApartmentRentalSystem.Application.Commands.Bookings;

// Команда -- контейнер данных, поэтому должна быть иммутабельной.
// IRequest<bool> означает что команда вернет true при успехе и false при ошибке
public record ApproveBookingCommand(Guid BookingId) : IRequest<bool>;


public class ApproveBookingCommandHandler : IRequestHandler<ApproveBookingCommand, bool>
{
    // Репозиторий + управление подключением
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveBookingCommandHandler(
        IBookingRepository bookingRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    // Метод отклонения заявки -- группирует логику отклонения воедино
    public async Task<bool> Handle(ApproveBookingCommand request, CancellationToken cancellationToken)
    {
        // 1. Начинаем транзакцию
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // 2. Получаем сущность через репозиторий (внедрен напрямую)
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                return false;
            }

            // 3. Вызываем доменный метод
            booking.Approve();

            // 4. Сохраняем через репозиторий
            await _bookingRepository.UpdateAsync(booking);

            // 5. Коммит
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
