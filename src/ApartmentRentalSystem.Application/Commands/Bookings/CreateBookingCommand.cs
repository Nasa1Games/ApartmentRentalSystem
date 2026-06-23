using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;

namespace ApartmentRentalSystem.Application.Commands.Bookings;

public record CreateBookingCommand(
    Guid TenantId,
    Guid ApartmentId,
    DateTime CheckInDate,
    DateTime CheckOutDate,
    int PartySize
) : IRequest<Guid>;


public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Guid>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IApartmentRepository _apartmentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IApartmentRepository apartmentRepository,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _apartmentRepository = apartmentRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Проверяем существование арендатора
            var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
            if (tenant == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Пользователь не найден");
            }

            // 2. Проверяем, не заблокирован ли пользователь
            if (tenant.IsBlocked)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Ваш аккаунт заблокирован. Обратитесь к администратору.");
            }

            // 3. Проверяем существование квартиры
            var apartment = await _apartmentRepository.GetByIdAsync(request.ApartmentId);
            if (apartment == null)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Квартира не найдена");
            }

            // 4. Проверяем статус квартиры
            if (apartment.Status != ApartmentStatus.Available)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException($"Квартира недоступна для бронирования (статус: {apartment.Status})");
            }

            // 5. Валидация дат
            if (request.CheckInDate >= request.CheckOutDate)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Дата выезда должна быть позже даты заезда");
            }
            if (request.CheckInDate.Date < DateTime.UtcNow.Date)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Нельзя забронировать квартиру на прошедшие даты");
            }

            // 6. Проверяем вместимость (опционально, если нужно)
            if (request.PartySize <= 0)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Количество гостей должно быть больше 0");
            }
            if (request.PartySize > apartment.Capacity)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Превышена вместимость квартиры. Максимум: {apartment.Capacity} чел., " +
                    $"указано: {request.PartySize} чел.");
            }

            // 7. Проверяем доступность квартиры на выбранные даты
            var isAvailable = await IsApartmentAvailableAsync(
                request.ApartmentId, 
                request.CheckInDate, 
                request.CheckOutDate, 
                cancellationToken);
            if (!isAvailable)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Квартира уже забронирована на выбранные даты");
            }

            // 8. Рассчитываем стоимость
            var nights = (request.CheckOutDate.Date - request.CheckInDate.Date).Days;
            var totalPrice = apartment.BasePricePerNight.Amount * nights;
            
            // Минимальная стоимость (например, 1 ночь)
            if (nights < 1)
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw new InvalidOperationException("Минимальный срок бронирования — 1 ночь");
            }

            // 9. Создаем бронь
            var booking = new Booking(
                tenantId: request.TenantId,
                apartmentId: request.ApartmentId,
                period: new DateRange(request.CheckInDate, request.CheckOutDate),
                totalPrice: new Money(totalPrice),
                deposit: new Money(apartment.BasePricePerNight.Amount) // Депозит = 1 ночь
            );

            await _bookingRepository.AddAsync(booking);
            await _unitOfWork.CommitAsync(cancellationToken);

            return booking.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Проверяет, свободна ли квартира на указанные даты
    /// </summary>
    private async Task<bool> IsApartmentAvailableAsync(
        Guid apartmentId, 
        DateTime checkIn, 
        DateTime checkOut,
        CancellationToken ct)
    {
        // Получаем все активные брони для этой квартиры
        var bookings = await _bookingRepository
            .GetByApartmentIdAsync(apartmentId);
        
        // Фильтруем только активные брони (Pending и Approved)
        var activeBookings = bookings
            .Where(b => b.Status is BookingStatus.Pending or BookingStatus.Approved);

        // Проверяем пересечение дат
        foreach (var booking in activeBookings)
        {
            // Даты пересекаются, если:
            // checkIn < existingCheckOut AND checkOut > existingCheckIn
            if (checkIn.Date < booking.Period.End.Date && 
                checkOut.Date > booking.Period.Start.Date)
            {
                return false; // Квартира занята
            }
        }
        return true; // Квартира свободна
    }
}