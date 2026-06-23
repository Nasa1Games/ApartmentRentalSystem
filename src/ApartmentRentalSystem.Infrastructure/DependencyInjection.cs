using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Infrastructure.Persistence.Db;
using ApartmentRentalSystem.Infrastructure.Persistence.Repositories;
using ApartmentRentalSystem.Infrastructure.Telegram;
using ApartmentRentalSystem.Application; // ← Новый using

namespace ApartmentRentalSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Сначала регистрируем Application-слой
        services.AddApplication();
        
        // 2. Фабрика подключений
        services.AddSingleton<IDbConnectionFactory>(sp =>
            new PostgresConnectionFactory(configuration));

        // 3. Репозитории
        services.AddScoped<IApartmentRepository, PostgresApartmentRepository>();
        services.AddScoped<IBookingRepository, PostgresBookingRepository>();
        services.AddScoped<ITenantRepository, PostgresTenantRepository>();
<<<<<<< Updated upstream
        services.AddScoped<ITransactionRepository, PostgresTransactionRepository>();
        services.AddScoped<INotificationRepository, PostgresNotificationRepository>();

        // 4. Telegram Bot (теперь зависит от ITelegramUserService из Application)
        services.AddSingleton<ITelegramBotService, TelegramBotService>();
=======
        services.AddScoped<IBookingQueryRepository, PostgresBookingQueryRepository>();
		services.AddScoped<INotificationDbConnectionFactory, NotificationDbConnectionFactoryAdapter>();
>>>>>>> Stashed changes

        return services;
    }
}