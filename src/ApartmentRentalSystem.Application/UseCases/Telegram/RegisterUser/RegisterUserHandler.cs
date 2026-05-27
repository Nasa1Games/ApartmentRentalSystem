using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Application.UseCases.Telegram.RegisterUser;

/// Обработчик команды регистрации пользователя через Телеграм
public class RegisterUserHandler
{
    private readonly ITenantRepository _tenantRepository;

    public RegisterUserHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command, CancellationToken ct = default)
    {
        // Проверяем, есть ли пользователь с таким TelegramId
        var existingUser = await _tenantRepository.GetByTelegramIdAsync(command.TelegramId);
        
        if (existingUser != null)
        {
            // Пользователь уже зарегистрирован
            return RegisterUserResult.Success(existingUser, isNewRegistration: false);
        }
        
        var placeholderPhone = $"+0_{command.TelegramId}";

        // Создаём нового пользователя
        var newTenant = new Tenant(
            email: $"{command.Username}@telegram.local",
            passwordHash: Guid.NewGuid().ToString("N"), // Временный хеш
            contact: new ContactInfo(
                fullName: command.Username,
                email: $"{command.Username}@telegram.local",
                phone: placeholderPhone), // Телефон можно запросить позже
            userRole: UserRole.User,
            telegramId: command.TelegramId,
            isBlocked: false);

        await _tenantRepository.AddAsync(newTenant);
        
        return RegisterUserResult.Success(newTenant, isNewRegistration: true);
    }
}