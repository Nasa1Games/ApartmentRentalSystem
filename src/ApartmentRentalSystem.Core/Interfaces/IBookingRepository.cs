using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Specifications;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<IEnumerable<Booking>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<Booking>> GetPendingAsync();
    Task<IEnumerable<Booking>> GetFilteredAsync(BookingFilterCriteria criteria);
    Task AddAsync(Booking booking);
    Task UpdateStatusAsync(Guid id, BookingStatus status, string? reason = null);
    Task DeleteAsync(Guid id);
}
