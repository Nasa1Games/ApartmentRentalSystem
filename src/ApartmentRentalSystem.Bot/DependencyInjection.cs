using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace ApartmentRentalSystem.Bot;

public static class DependencyInjection
{
    public static IServiceCollection AddBot(this IServiceCollection services, IConfiguration config)
    {
        // Получаем токен из конфигурации
        var botToken = config["TelegramBot:Token"];
        
        if (string.IsNullOrWhiteSpace(botToken))
        {
            throw new InvalidOperationException("Telegram Bot Token not configured");
        }

        // Регистрируем Telegram Bot Client
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botToken));
        
        // Регистрируем сервис бота
        services.AddScoped<TelegramBotService>();
        
        return services;
    }
}