using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Entities;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ApartmentId { get; private set; }
    public DateRange Period { get; private set; }
    public BookingStatus Status { get; private set; }
    public Money TotalPrice { get; private set; }
    public Money Deposit { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Booking() { } // Для Dapper

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

    public void Approve()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Можно подтвердить только_pending бронь");
        
        Status = BookingStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException("Можно отклонить только pending бронь");
        
        Status = BookingStatus.Rejected;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status is not (BookingStatus.Pending or BookingStatus.Approved))
            throw new InvalidOperationException("Можно отменить только pending или approved бронь");
        
        Status = BookingStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != BookingStatus.Approved)
            throw new InvalidOperationException("Можно завершить только approved бронь");
        
        Status = BookingStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }
}