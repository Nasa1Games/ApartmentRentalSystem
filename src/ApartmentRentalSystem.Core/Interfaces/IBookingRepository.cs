using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<IEnumerable<Booking>> GetByTenantIdAsync(Guid tenantId);
    Task<IEnumerable<Booking>> GetByApartmentIdAsync(Guid apartmentId);
    Task<IEnumerable<Booking>> GetPendingAsync();
    Task<IEnumerable<Booking>> GetFilteredAsync(BookingFilterCriteria criteria);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(Guid id);
}
