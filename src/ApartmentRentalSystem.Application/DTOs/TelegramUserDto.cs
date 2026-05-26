using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Application.DTOs;

/// DTO для передачи данных о пользователе во внешние слои
public record TelegramUserDto(
    Guid Id,
    string FullName,
    string Email,
    long? TelegramId,  
    bool IsBlocked,
    UserRole UserRole);