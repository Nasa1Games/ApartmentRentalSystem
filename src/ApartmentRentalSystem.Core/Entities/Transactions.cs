using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid BookingId { get; private set; }
    public Money Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Transaction() { } // Для Dapper

    public Transaction(
        Guid id,
        Guid bookingId,
        Money amount,
        TransactionType type,
        TransactionStatus status,
        DateTime? processedAt)
    {
        Id = id;
        BookingId = bookingId;
        Amount = amount;
        Type = type;
        Status = status;
        ProcessedAt = processedAt;
    }

    public Transaction(
        Guid bookingId,
        Money amount,
        TransactionType type)
    {
        if (amount.Amount <= 0)
            throw new ArgumentException("Transaction amount must be positive", nameof(amount));

        Id = Guid.NewGuid();
        BookingId = bookingId;
        Amount = amount;
        Type = type;
        Status = TransactionStatus.Pending;
    }

    public void Complete()
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException($"Можно завершить только транзакцию в статусе Pending. Текущий статус: {Status}");
        
        Status = TransactionStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Fail(string? reason = null)
    {
        if (Status != TransactionStatus.Pending)
            throw new InvalidOperationException($"Можно отклонить только транзакцию в статусе Pending. Текущий статус: {Status}");
        
        Status = TransactionStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
    }

    public bool IsProcessed() => Status is TransactionStatus.Completed or TransactionStatus.Failed;
}