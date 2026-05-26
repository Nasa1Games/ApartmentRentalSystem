namespace ApartmentRentalSystem.Core.ValueObjects;

public record ContactInfo
{
    public string FullName { get; init; }
    public string Email { get; init; }
    public string Phone { get; init; }

    public ContactInfo(string fullName, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Имя обязательно", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email обязателен", nameof(email));
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Телефон обязателен", nameof(phone));

        FullName = fullName;
        Email = email;
        Phone = phone;
    }
}