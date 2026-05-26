using ApartmentRentalSystem.Application.DTOs;

namespace ApartmentRentalSystem.Application.Services;

/// Сервис для работы с пользователями Телеграма
public interface ITelegramUserService
{
    /// Регистрирует или находит пользователя по Telegram ID
    Task<TelegramUserResult> GetOrCreateUserAsync(long telegramId, string username, CancellationToken ct = default);
}