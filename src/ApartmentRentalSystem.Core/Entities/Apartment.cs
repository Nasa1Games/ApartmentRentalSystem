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

    private Apartment() { } // Для Dapper/EF Core

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

    public void UpdateDetails(
        int entrance,
        int floor,
        int capacity,
        Money basePricePerNight,
        string? description)
    {
        Entrance = entrance;
        Floor = floor;
        Capacity = capacity;
        BasePricePerNight = basePricePerNight;
        Description = description;
    }

    public void ChangeStatus(ApartmentStatus status)
    {
        Status = status;
    }

    public void MarkAsMaintenance() => Status = ApartmentStatus.Maintenance;
    public void MarkAsAvailable() => Status = ApartmentStatus.Available;
}


