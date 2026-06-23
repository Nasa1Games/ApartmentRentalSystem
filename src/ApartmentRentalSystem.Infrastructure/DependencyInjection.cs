using ApartmentRentalSystem.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Infrastructure.Db;
using ApartmentRentalSystem.Infrastructure.Db.Connection;
using ApartmentRentalSystem.Infrastructure.Db.Repositories;
using ApartmentRentalSystem.Infrastructure.Messaging;
using ApartmentRentalSystem.Application.Interfaces;

namespace ApartmentRentalSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Фабрика подключений
        services.AddSingleton<IDbConnectionFactory>(
            new PostgresConnectionFactory(configuration));
        
        // 2. UnitOfWork и репозитории
        services.AddScoped<IUnitOfWork, PostgresUnitOfWork>();
        
        services.AddScoped<IApartmentRepository, PostgresApartmentRepository>();
        services.AddScoped<IBookingRepository, PostgresBookingRepository>();
        services.AddScoped<ITenantRepository, PostgresTenantRepository>();
        services.AddScoped<IBookingQueryRepository, PostgresBookingQueryRepository>();

        // 3. Отправка уведомлений.
        // Регистрируем Роутер как основной интерфейс отправки
        services.AddScoped<INotificationSender, NotificationRouter>();
        
        // Регистрируем конкретные реализации отправки
        services.AddScoped<ITelegramSender, TelegramSender>();
        services.AddScoped<IEmailSender, EmailSender>();
        
        
        return services;
    }
}