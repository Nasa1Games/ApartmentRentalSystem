using ApartmentRentalSystem.Core.Entities;

namespace ApartmentRentalSystem.Application.UseCases.Telegram.RegisterUser;

/// Результат выполнения команды регистрации
public class RegisterUserResult
{
    public bool IsSuccess { get; }
    public Tenant? User { get; }
    public bool IsNewRegistration { get; }
    public string? ErrorMessage { get; }

    private RegisterUserResult(bool isSuccess, Tenant? user, bool isNewRegistration, string? errorMessage)
    {
        IsSuccess = isSuccess;
        User = user;
        IsNewRegistration = isNewRegistration;
        ErrorMessage = errorMessage;
    }

    public static RegisterUserResult Success(Tenant user, bool isNewRegistration) =>
        new(true, user, isNewRegistration, null);

    public static RegisterUserResult Failure(string error) =>
        new(false, null, false, error);
}