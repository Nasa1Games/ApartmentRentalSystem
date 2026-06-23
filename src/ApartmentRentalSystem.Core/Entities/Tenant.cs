using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string? Email { get; private set; }
    public ContactInfo Contact { get; private set; }
    public long TelegramId { get; private set; }
    public NotificationChannel PreferredChannel { get; private set; } 
    public bool IsBlocked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    public Tenant(
        Guid id,
        string email,
        ContactInfo contact,
        long telegramId,
        NotificationChannel preferredChannel,
        bool isBlocked,
        DateTime createdAt)
    {
        Id = id;
        Email = email;
        Contact = contact;
        TelegramId = telegramId;
        PreferredChannel = preferredChannel;
        IsBlocked = isBlocked;
        CreatedAt = createdAt;
    }

    public Tenant(
        string email, 
        ContactInfo contact, 
        long telegramId, 
        NotificationChannel preferredChannel,
        bool isBlocked = false)
    {
        Id = Guid.NewGuid();
        Email = email;
        Contact = contact;
        TelegramId = telegramId;
        PreferredChannel = preferredChannel;
        IsBlocked = isBlocked;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTelegramId(long telegramId)
    {
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId должен быть положительным", nameof(telegramId));
        TelegramId = telegramId;
    }
    
    public void UpdateContact(string? fullName, string? phone)
    {
        if (!string.IsNullOrWhiteSpace(fullName))
            Contact = Contact with { FullName = fullName };
        if (!string.IsNullOrWhiteSpace(phone))
            Contact = Contact with { Phone = phone };
    }

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));
        Email = email;
    }

    /// <summary>
    /// Метод для смены предпочтительного способа отправки уведомлений
    /// </summary>
    public void SetNotificationPreference(NotificationChannel channel)
    {
        PreferredChannel = channel;
    }
    
    public void Block() => IsBlocked = true;
    public void Unblock() => IsBlocked = false;
}