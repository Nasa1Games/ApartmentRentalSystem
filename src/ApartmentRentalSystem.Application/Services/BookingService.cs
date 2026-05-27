using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IApartmentRepository _apartmentRepo;

    public BookingService(
        IBookingRepository bookingRepo, 
        IApartmentRepository apartmentRepo)
    {
        _bookingRepo = bookingRepo;
        _apartmentRepo = apartmentRepo;
    }

    public async Task<Booking> CreateAsync(
        Guid tenantId, 
        Guid apartmentId, 
        DateRange period, 
        CancellationToken ct = default)
    {
        // Проверяем существование и доступность квартиры
        var apartment = await _apartmentRepo.GetByIdAsync(apartmentId)
            ?? throw new InvalidOperationException("Квартира не найдена");

        if (apartment.Status != ApartmentStatus.Available)
            throw new InvalidOperationException("Квартира сейчас недоступна для бронирования");
        
        var totalPrice = apartment.BasePricePerNight * period.Days;
        
        var booking = new Booking(
            tenantId: tenantId,
            apartmentId: apartmentId,
            period: period,
            totalPrice: totalPrice,
            deposit: Money.Zero);
        
        await _bookingRepo.AddAsync(booking);

        return booking;
    }

    public async Task<IEnumerable<Booking>> GetPendingBookingsAsync(CancellationToken ct = default)
        => await _bookingRepo.GetPendingAsync();

    public async Task<Booking> ApproveBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) 
            ?? throw new InvalidOperationException("Бронь не найдена");
        
        booking.Approve();
        await _bookingRepo.UpdateStatusAsync(booking.Id, booking.Status);
        return booking;
    }

    public async Task<Booking> RejectBookingAsync(Guid bookingId, string reason, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId) 
            ?? throw new InvalidOperationException("Бронь не найдена");
        
        booking.Reject(reason); 
        await _bookingRepo.UpdateStatusAsync(booking.Id, booking.Status, reason);
        return booking;
    }
    
    public async Task<IEnumerable<Booking>> GetByTenantIdAsync(Guid tenantId, CancellationToken ct = default)
        => await _bookingRepo.GetByTenantIdAsync(tenantId);
    
    public async Task CancelBookingAsync(Guid bookingId, Guid tenantId, CancellationToken ct = default)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId)
                      ?? throw new InvalidOperationException("Бронь не найдена");
        
        if (booking.TenantId != tenantId)
            throw new InvalidOperationException("Вы не можете отменить чужую бронь");
        
        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
            throw new InvalidOperationException("Эту бронь уже нельзя отменить");
        
        booking.Reject("Отменено пользователем");
        
        await _bookingRepo.UpdateStatusAsync(booking.Id, booking.Status, "Отменено пользователем");
    }
} 