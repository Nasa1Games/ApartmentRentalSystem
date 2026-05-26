using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<IEnumerable<Transaction>> GetByBookingIdAsync(Guid bookingId);
    Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status);
    Task<IEnumerable<Transaction>> GetFilteredAsync(
        Guid? bookingId = null, 
        TransactionStatus? status = null, 
        TransactionType? type = null,
        DateTime? fromDate = null, 
        DateTime? toDate = null);
    
    Task AddAsync(Transaction transaction);
    Task UpdateStatusAsync(Guid id, TransactionStatus status);
    Task DeleteAsync(Guid id);
}