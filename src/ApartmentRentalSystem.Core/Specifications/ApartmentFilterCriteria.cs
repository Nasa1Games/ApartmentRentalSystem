namespace ApartmentRentalSystem.Core.Specifications;

public record ApartmentFilterCriteria
{
    public int? MinFloor { get; init; }
    public int? MaxFloor { get; init; }
    public int? MinCapacity { get; init; }
    public string? Status { get; init; }
    public int? MinPrice { get; init; }
    public int? MaxPrice { get; init; }
}