using ApartmentRentalSystem.Application.DTOs;
using ApartmentRentalSystem.Application.UseCases.Telegram.RegisterUser;

namespace ApartmentRentalSystem.Application.Services;

public class TelegramUserService : ITelegramUserService
{
    private readonly RegisterUserHandler _registerHandler;

    public TelegramUserService(RegisterUserHandler registerHandler)
    {
        _registerHandler = registerHandler;
    }

    public async Task<TelegramUserResult> GetOrCreateUserAsync(
        long telegramId, 
        string username, 
        CancellationToken ct = default)
    {
        var command = new RegisterUserCommand(telegramId, username);
        var result = await _registerHandler.HandleAsync(command, ct);

        if (!result.IsSuccess)
        {
            return TelegramUserResult.Failure(result.ErrorMessage!);
        }

        // Маппинг доменной сущности в DTO для внешнего слоя
        var dto = new TelegramUserDto(
            Id: result.User!.Id,
            FullName: result.User.Contact.FullName,
            Email: result.User.Email,
            TelegramId: result.User.TelegramId,
            IsBlocked: result.User.IsBlocked,
            UserRole: result.User.UserRole);

        return TelegramUserResult.Success(dto, result.IsNewRegistration);
    }
}