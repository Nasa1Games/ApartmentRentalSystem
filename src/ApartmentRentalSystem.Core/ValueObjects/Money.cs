namespace ApartmentRentalSystem.Core.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "RUB";

    public Money(decimal amount, string currency = "RUB")
    {
        if (amount < 0)
            throw new ArgumentException("Сумма не может быть отрицательной", nameof(amount));
        
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero => new(0);
    
    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount, a.Currency);
    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount, a.Currency);
    public static Money operator *(Money m, decimal factor) => new(m.Amount * factor, m.Currency);
}