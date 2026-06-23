using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Events;
using ApartmentRentalSystem.Core.ValueObjects;
using MediatR;


namespace ApartmentRentalSystem.Core.Entities;

public class Booking : Entity
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ApartmentId { get; private set; }
    public DateRange Period { get; private set; }
    public Money TotalPrice { get; private set; }
    public Money Deposit { get; private set; }
    public BookingStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    public Booking(
        Guid id, 
        Guid tenantId, 
        Guid apartmentId, 
        DateRange period,
        Money totalPrice, 
        Money deposit, 
        BookingStatus status,
        string? cancellationReason, 
        DateTime createdAt, 
        DateTime? updatedAt)
    {
        Id = id; 
        TenantId = tenantId; 
        ApartmentId = apartmentId; 
        Period = period;
        TotalPrice = totalPrice; 
        Deposit = deposit; 
        Status = status;
        CancellationReason = cancellationReason; 
        CreatedAt = createdAt; 
        UpdatedAt = updatedAt;
    }
    
    public Booking(
        Guid tenantId,
        Guid apartmentId,
        DateRange period,
        Money totalPrice,
        Money? deposit = null)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        ApartmentId = apartmentId;
        Period = period;
        TotalPrice = totalPrice;
        Deposit = deposit ?? Money.Zero;
        Status = BookingStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отмечает аренду подтвержденной (согласованной)
    /// </summary>
    public void Approve()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Подтверждать можно только pending бронь");
        
        Status = BookingStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BookingApprovedEvent(
            Id, TenantId, ApartmentId, DateTime.UtcNow, Period, TotalPrice
        ));
    }

    /// <summary>
    /// Отказ клиенту в аренде
    /// </summary>
    public void Reject(string reason)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Можно отклонить только pending бронь");
        
        Status = BookingStatus.Rejected;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BookingRejectedEvent(
            Id, TenantId, ApartmentId, reason, DateTime.UtcNow
        ));
    }
    
    /// <summary>
    /// Отказ клиентом от аренды
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Approved))
            throw new InvalidOperationException("Можно отменить только pending или approved бронь");
        
        Status = BookingStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
        
        AddDomainEvent(new BookingCancelledEvent(
            Id, TenantId, ApartmentId, reason, DateTime.UtcNow
        ));
    }
    
    /// <summary>
    /// Отмечает сделку аренды завершенной
    /// </summary>
    public void Complete()
    {
        if (Status != BookingStatus.Approved)
            throw new InvalidOperationException("Можно завершить только approved бронь");
        
        Status = BookingStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
        
        // Опционально можно добавить уведомление админу
        // AddDomainEvent(new BookingCompletedEvent(
        //     Id, TenantId, ApartmentId, DateTime.UtcNow, TotalPrice
        // ));
    }
}