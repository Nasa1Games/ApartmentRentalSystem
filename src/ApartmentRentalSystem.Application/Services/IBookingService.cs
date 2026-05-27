using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Application.Services;

// Сервис для управления бронированиями.
// Реализует бизнес-правила создания, изменения и чтения броней.
public interface IBookingService
{
    Task<Booking> CreateAsync(
        Guid tenantId, 
        Guid apartmentId, 
        DateRange period, 
        CancellationToken ct = default);
    
    Task<IEnumerable<Booking>> GetPendingBookingsAsync(
        CancellationToken ct = default);
    
    Task<Booking> ApproveBookingAsync(
        Guid bookingId, 
        CancellationToken ct = default);
    
    Task<Booking> RejectBookingAsync(
        Guid bookingId, string reason, 
        CancellationToken ct = default);
    
    Task<IEnumerable<Booking>> GetByTenantIdAsync(
        Guid tenantId, CancellationToken ct = default);
    
    Task CancelBookingAsync(
        Guid bookingId, 
        Guid tenantId, 
        CancellationToken ct = default);
}