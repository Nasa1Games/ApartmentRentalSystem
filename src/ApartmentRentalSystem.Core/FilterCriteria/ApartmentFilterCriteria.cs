using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.FilterCriteria;

public record ApartmentFilterCriteria
{
    public int? MinFloor { get; set; }
    public int? MaxFloor { get; set; }
    public int? MinCapacity { get; set; }
    public ApartmentStatus? Status { get; set; }
    public Money? MinPrice { get; set; }
    public Money? MaxPrice { get; set; }
}