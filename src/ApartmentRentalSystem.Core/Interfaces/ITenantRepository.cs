using ApartmentRentalSystem.Core.Entities;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetByEmailAsync(string email);
    Task<Tenant?> GetByTelegramIdAsync(long telegramId);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task DeleteAsync(Guid id);
}