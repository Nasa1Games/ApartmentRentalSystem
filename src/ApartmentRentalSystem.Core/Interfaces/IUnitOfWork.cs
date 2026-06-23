using System.Data;
using MediatR;
using Npgsql;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface IUnitOfWork
{
    // 1. Доступ к бд
    NpgsqlConnection? Connection { get; }
    
    // 2. Управление транзакцией
    NpgsqlTransaction? Transaction { get; }
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
    
    // 3. Механизм для событий
    void RegisterDomainEvents(IEnumerable<INotification> events);
}