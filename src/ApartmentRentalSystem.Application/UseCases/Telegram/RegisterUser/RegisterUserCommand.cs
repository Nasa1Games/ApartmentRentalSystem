namespace ApartmentRentalSystem.Application.UseCases.Telegram.RegisterUser;

/// Команда для регистрации/поиска пользователя по Telegram ID
public record RegisterUserCommand(
    long TelegramId,
    string Username);