namespace ApartmentRentalSystem.Application.DTOs;

/// Результат операций с пользователем Телеграма
public class TelegramUserResult
{
    public bool IsSuccess { get; }
    public TelegramUserDto? User { get; }
    public bool IsNewRegistration { get; }
    public string? ErrorMessage { get; }

    private TelegramUserResult(bool isSuccess, TelegramUserDto? user, bool isNewRegistration, string? errorMessage)
    {
        IsSuccess = isSuccess;
        User = user;
        IsNewRegistration = isNewRegistration;
        ErrorMessage = errorMessage;
    }

    public static TelegramUserResult Success(TelegramUserDto user, bool isNewRegistration) =>
        new(true, user, isNewRegistration, null);

    public static TelegramUserResult Failure(string error) =>
        new(false, null, false, error);
}