using Microsoft.Extensions.DependencyInjection;
using ApartmentRentalSystem.Application.Services;
using ApartmentRentalSystem.Application.UseCases.Telegram.RegisterUser;

namespace ApartmentRentalSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // UseCases
        services.AddScoped<RegisterUserHandler>();
        
        // Services
        services.AddScoped<ITelegramUserService, TelegramUserService>();
        
        return services;
    }
}