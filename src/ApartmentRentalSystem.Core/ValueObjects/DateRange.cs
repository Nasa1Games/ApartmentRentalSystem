namespace ApartmentRentalSystem.Core.ValueObjects;

public record DateRange
{
    public DateTime Start { get; init; }
    public DateTime End { get; init; }

    public DateRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new ArgumentException("Дата окончания должна быть позже даты начала");
        
        Start = start.Date;
        End = end.Date;
    }

    public int Days => (End - Start).Days;

    public bool OverlapsWith(DateRange other)
    {
        return Start < other.End && other.Start < End;
    }
}