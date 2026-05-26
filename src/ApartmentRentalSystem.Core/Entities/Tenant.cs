using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Core.Entities;

public class Tenant
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public ContactInfo Contact { get; private set; }
    public long? TelegramId { get; private set; }
    public void LinkTelegram(long telegramId) => TelegramId = telegramId; // вероятно подлежит удалению
    public bool IsBlocked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public UserRole UserRole  { get; private set; }

    private Tenant() { } // Для Dapper
    
    public Tenant(
        Guid id,
        string email,
        string passwordHash,
        ContactInfo contact,
        UserRole userRole,
        long telegramId,
        bool isBlocked,
        DateTime createdAt)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Contact = contact;
        UserRole = userRole;
        TelegramId = telegramId;
        IsBlocked = isBlocked;
        CreatedAt = createdAt;
    }

    public Tenant(string email, string passwordHash, ContactInfo contact, UserRole userRole, long telegramId, bool isBlocked = false)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Contact = contact;
        TelegramId = telegramId;
        IsBlocked = isBlocked;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateTelegramId(long telegramId)
    {
        if (telegramId <= 0)
            throw new ArgumentException("TelegramId должен быть положительным", nameof(telegramId));
        TelegramId = telegramId;
    }
    
    public void Block() => IsBlocked = true;
    public void Unblock() => IsBlocked = false;
}