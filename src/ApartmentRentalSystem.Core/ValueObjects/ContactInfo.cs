namespace ApartmentRentalSystem.Core.ValueObjects;

public record ContactInfo
{
    public string FullName { get; init; }
    public string? Phone { get; init; }

    public ContactInfo(string fullName, string phone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Имя обязательно", nameof(fullName));

        FullName = fullName;
        Phone = phone;
    }
}