using MediatR;

namespace ApartmentRentalSystem.Core.Entities;

public abstract class Entity
{
    // Приватный список, где сущность хранит свои события
    private readonly List<INotification> _domainEvents = [];

    // Публичное свойство только для чтения, чтобы репозиторий мог их достать
    public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

    // Добавляет событие в список
    protected void AddDomainEvent(INotification @event) => _domainEvents.Add(@event);

    // Метод для очистки событий после того, как UnitOfWork их опубликует
    public void ClearDomainEvents() => _domainEvents.Clear();
}