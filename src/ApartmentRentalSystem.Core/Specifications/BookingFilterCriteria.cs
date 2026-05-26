namespace ApartmentRentalSystem.Core.Specifications;

public record BookingFilterCriteria
{
    public Guid? TenantId { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}