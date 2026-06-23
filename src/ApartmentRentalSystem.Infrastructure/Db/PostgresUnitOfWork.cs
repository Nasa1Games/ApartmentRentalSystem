using System.Data;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Infrastructure.Db.Connection;
using MediatR;
using Npgsql;

namespace ApartmentRentalSystem.Infrastructure.Db;

// Принцип работы UnitOfWork:
// Bookings.GetByIdAsync            <- отмечается как транзакция
// booking.Approve();               <- в объект добавляется событие-оповещение
// Bookings.UpdateAsync(booking)    <- внутри апдейта переписываем событие из объекта в свой список
// ...
// также другая логика, которая может пополнять наш список событий,
// например с таблицей Apartments
// (когда все завершили, выполняем коммит)
// ...
// await _unitOfWork.CommitAsync(); <- сохраняются транзакции + передается управление событиям (события ищет DI)
public class PostgresUnitOfWork : IUnitOfWork
{
    // - - 1. Подключение к бд - -
    // _factory -- класс для создания подключения к бд (возвращает _connection)
    private readonly IDbConnectionFactory _factory;
    
    // _connection -- объект подключения к бд
    private NpgsqlConnection? _connection;
    public NpgsqlConnection? Connection
    {
        get
        {
            if (_connection != null) return _connection;
            
            _connection = _factory.CreateConnection();
            if (_connection.State != ConnectionState.Open)
                _connection.Open();
            return _connection;
        }
    }
    
    
    // - - 2. Создаем список транзакций - -
    // _transaction -- копит все совершенные в бд изменения
    private NpgsqlTransaction? _transaction;
    public NpgsqlTransaction? Transaction => _transaction;
    
    
    // - - 3. Настраиваем рассылку - -
    // _mediator -- класс-курьер,
    // который передает события всем подписчикам в app-слой (например в BookingAprooveNotificationHandler),
    // затем в app формируется ответ и отправляет его через INotificationSender (NotificationRouter).
    private readonly IMediator _mediator;
    
    // _pendingEvents -- список случившихся событий (подписчики сопоставлены событиям в DI)
    private readonly List<INotification> _pendingEvents = [];
    
    
    public PostgresUnitOfWork(
        IDbConnectionFactory factory,
        IMediator mediator)
    {
        _factory = factory;
        _mediator = mediator;
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null) return; // Уже начата
        _transaction = await Connection.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction == null) return;
        try
        {
            // 1. Фиксируем изменения в БД
            await _transaction.CommitAsync(ct);
            
            // 2. Только после успешного коммита публикуем события
            foreach (var evt in _pendingEvents)
            {
                await _mediator.Publish(evt, ct);
            }
            _pendingEvents.Clear();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction == null) return;
        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
            _pendingEvents.Clear(); // События не отправляем при откате!
        }
    }

    public void RegisterDomainEvents(IEnumerable<INotification> events)
    {
        _pendingEvents.AddRange(events);
    }
}