using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.FilterCriteria;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface IApartmentRepository
{
    Task<Apartment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Apartment>> GetAllAsync();
    Task<IEnumerable<Apartment>> GetFilteredAsync(ApartmentFilterCriteria criteria);
    Task AddAsync(Apartment apartment);
    Task UpdateAsync(Apartment apartment);
    Task DeleteAsync(Guid id);
}