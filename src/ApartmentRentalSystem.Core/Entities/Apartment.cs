using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Entities;

public class Apartment
{
    public Guid Id { get; private set; }
    public string UnitNumber { get; private set; }
    public int Entrance { get; private set; }
    public int Floor { get; private set; }
    public int Capacity { get; private set; }
    public Money BasePricePerNight { get; private set; }
    public string? Description { get; private set; }
    public ApartmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Apartment(
        Guid id,
        string unitNumber,
        int entrance,
        int floor,
        int capacity,
        Money basePricePerNight,
        string? description,
        ApartmentStatus status,
        DateTime createdAt
    )
    {
        Id = id;
        UnitNumber = unitNumber;
        Entrance = entrance;
        Floor = floor;
        Capacity = capacity;
        BasePricePerNight = basePricePerNight;
        Description = description;
        Status = status;
        CreatedAt = createdAt;
    }
    
    public Apartment(
        string unitNumber,
        int entrance,
        int floor,
        int capacity,
        Money basePricePerNight,
        string? description = null)
    {
        Id = Guid.NewGuid();
        UnitNumber = unitNumber;
        Entrance = entrance;
        Floor = floor;
        Capacity = capacity;
        BasePricePerNight = basePricePerNight;
        Description = description;
        Status = ApartmentStatus.Available;
        CreatedAt = DateTime.UtcNow;
    }
    
    public void Update(
        string? unitNumber = null,
        int? entrance = null,
        int? floor = null,
        int? capacity = null,
        Money? basePricePerNight = null,
        string? description = null)
    {
        if (!string.IsNullOrWhiteSpace(unitNumber))
            UnitNumber = unitNumber;
        if (entrance.HasValue && entrance > 0)
            Entrance = entrance.Value;
        if (floor.HasValue && floor > 0)
            Floor = floor.Value;
        if (capacity.HasValue && capacity > 0)
            Capacity = capacity.Value;
        if (basePricePerNight != null && basePricePerNight.Amount > 0)
            BasePricePerNight = basePricePerNight;
        if (description != null)
            Description = description;
    }

    public void ChangeStatus(ApartmentStatus status)
    {
        Status = status;
    }

    public void MarkAsMaintenance() => Status = ApartmentStatus.Maintenance;
    public void MarkAsAvailable() => Status = ApartmentStatus.Available;
}


