using ApartmentRentalSystem.Application;
using ApartmentRentalSystem.Application.Commands.Apartments;
using ApartmentRentalSystem.Application.Commands.Bookings;
using ApartmentRentalSystem.Application.Commands.Tenants;
using ApartmentRentalSystem.Application.Queries.Apartments;
using ApartmentRentalSystem.Application.Queries.Bookings;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Infrastructure;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ApartmentRentalSystem.Bot;
using ApartmentRentalSystem.Bot.Services;

namespace ApartmentRentalSystem.Host;

public static class Program
{
    public static async Task Main(string[] args)
    {

        // 1. Настраиваем конфигурацию
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // 2. Создаём DI контейнер
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        
        // 3. Регистрируем слои
        services.AddInfrastructure(configuration);
        services.AddApplication();
        services.AddBot(configuration);
        services.AddSingleton<UserSessionService>(); 
        
        // 4. Настраиваем логирование
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // 5. Строим провайдер
        var serviceProvider = services.BuildServiceProvider();

        // 6. Запускаем приложение
        System.Console.WriteLine("╔══════════════════════════════════════╗");
        System.Console.WriteLine("║   ApartmentRentalSystem Host         ║");
        System.Console.WriteLine("╚══════════════════════════════════════╝");
        System.Console.WriteLine("\n✅ Приложение запущено. Нажмите Ctrl+C для выхода.\n");        

        // 7. Ждём завершения (для бота это будет бесконечный цикл)
        var cts = new CancellationTokenSource();
        
        // Обработка Ctrl+C
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        try
        {
            var botService = serviceProvider.GetRequiredService<TelegramBotService>();
            await botService.StartAsync(cts.Token);
            Console.WriteLine("🤖 Бот работает и слушает сообщения...\n");
            
            await Task.Delay(Timeout.Infinite, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n Приложение остановлено пользователем.");
        }
    }
    
    
    // Тесты
    public static async Task Main_test()
    {
        // Инициализация конфигурации
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        
        // Сборка DI-контейнера
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(builder =>
        {
            builder.AddConsole(); // Вывод в консоль
            builder.SetMinimumLevel(LogLevel.Information); // Минимальный уровень
        });      
        
        services.AddInfrastructure(config);
        services.AddApplication();

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var cts = new CancellationTokenSource(); // позволяет безопасно завершать процессы
        
        System.Console.WriteLine("\n\n = = = Starting tests = = =\n");
        
        // await RunDatabaseTestsAsync(provider, cts.Token);
        await RunBusinessLogicTestsAsync(provider, mediator, cts.Token);
    }
    
    
    // =============================================================================
    // Тестирование операций с БД
    // =============================================================================
    private static async Task RunDatabaseTestsAsync(
        IServiceProvider provider, 
        CancellationToken ct)
    {
        System.Console.WriteLine("Starting database tests...");
        
        var unitOfWork = provider.GetRequiredService<IUnitOfWork>();
        
        // DI находит реализации репозиториев
        var tenantRepo = provider.GetRequiredService<ITenantRepository>();
        var aptRepo = provider.GetRequiredService<IApartmentRepository>();
        var bookingRepo = provider.GetRequiredService<IBookingRepository>();
        System.Console.WriteLine("\nDI created\n");
        
        // создаем тестовую почту и номер квартиры
        var suffix = Guid.NewGuid().ToString()[..6];
        var testEmail = $"test_{suffix}@rental.com";
        var unitNumber = $"APT-{suffix}";
        
        // Создаем тестовые записи:
        var createdTenant = new Tenant(
            email: testEmail,
            contact: new ContactInfo(
                "Тест Тестов",
                "+79022651122"),
            telegramId: 123456789L,
            preferredChannel: NotificationChannel.Email,  // отсутствует реализация поэтому отлично подходит для тестов
            isBlocked: false);
        
        var createdApartment = new Apartment(
            unitNumber: unitNumber,
            entrance: 2,
            floor: 5,
            capacity: 3,
            basePricePerNight: new Money(4000),
            description: "Тестовая квартира");
        
        var createdBooking = new Booking(
            tenantId: createdTenant.Id,
            apartmentId: createdApartment.Id,
            period: new DateRange(DateTime.Today.AddDays(2), DateTime.Today.AddDays(6)),
            totalPrice: new Money(16000),
            deposit: new Money(2000));
        
        try  // - - Add - -
        {
            System.Console.WriteLine("Adding data...");
            await unitOfWork.BeginTransactionAsync(ct);

            await tenantRepo.AddAsync(createdTenant);
            System.Console.WriteLine($"Tenant создан с ID: {createdTenant.Id}");

            await aptRepo.AddAsync(createdApartment);
            System.Console.WriteLine($"Apartment создан с ID: {createdApartment.Id}");

            await bookingRepo.AddAsync(createdBooking);
            System.Console.WriteLine($"Booking создан с ID: {createdBooking.Id}");
            
            await unitOfWork.CommitAsync(ct);
            System.Console.WriteLine("Транзакция успешно закоммичена !\n");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\nОШИБКА: {ex.Message}");
            System.Console.WriteLine($"Стек: {ex.StackTrace}");
        
            // Откатываем транзакцию при ошибке
            await unitOfWork.RollbackAsync(ct);
        }
        
        try  // - - Read - -
        {
            // Tenant
            System.Console.WriteLine("Reading data...");
            await unitOfWork.BeginTransactionAsync(ct);

            var loadedTenant = await tenantRepo.GetByIdAsync(createdTenant.Id);
            System.Console.WriteLine($"Tenant by ID: {loadedTenant?.Id}");
            
            loadedTenant = await tenantRepo.GetByEmailAsync(createdTenant.Email);
            System.Console.WriteLine($"Tenant by Email: {loadedTenant?.Email}");
            
            loadedTenant = await tenantRepo.GetByTelegramIdAsync(createdTenant.TelegramId);
            System.Console.WriteLine($"Tenant by Tg_ID: {loadedTenant?.TelegramId}");
            
            var tenants = await tenantRepo.GetAllAsync();
            System.Console.WriteLine($"Tenant (all): {tenants.Count()}");
            
            // Apartment
            var aptById = await aptRepo.GetByIdAsync(createdApartment.Id);
            System.Console.WriteLine($"Apartment by ID: {aptById?.UnitNumber} ({aptById?.BasePricePerNight.Amount})");
            
            var apts = await aptRepo.GetAllAsync();
            System.Console.WriteLine($"Apartments (all): {apts.Count()}");
            
            var apartmentFilterCriteria = new ApartmentFilterCriteria
            {
                MinFloor = 5, MaxFloor = 7, 
                MinPrice = Money.Zero, MaxPrice = new Money(4000),
                Status = ApartmentStatus.Available, MinCapacity = 1
            };
            var apt = await aptRepo.GetFilteredAsync(apartmentFilterCriteria);
            System.Console.WriteLine($"Apartment criteria: {apt.Count()}");
            
            // Booking
            var pending = await bookingRepo.GetByIdAsync(createdBooking.Id);
            System.Console.WriteLine($"Pending bookings by ID: {pending.Id}");
            
            var pendings = await bookingRepo.GetByTenantIdAsync(createdTenant.Id);
            System.Console.WriteLine($"Pending bookings by Tenant: {pendings.Count()}");
            
            pendings = await bookingRepo.GetPendingAsync();
            System.Console.WriteLine($"Pending bookings: {pendings.Count()}");
            
            var bookingFilterCriteria = new BookingFilterCriteria
            {
                TenantId = createdTenant.Id,
                Status = BookingStatus.Pending.ToString()
            };
            pendings = await bookingRepo.GetFilteredAsync(bookingFilterCriteria);
            System.Console.WriteLine($"Filtered pendings: {pendings}");
            
            // Для чтения это не обязательно, но для идентичности добавим
            await unitOfWork.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\nОШИБКА: {ex.Message}");
            System.Console.WriteLine($"Стек: {ex.StackTrace}");
        
            // Откатываем транзакцию при ошибке
            await unitOfWork.RollbackAsync(ct);
        }
        
        try  // - - Delete - -
        {
            System.Console.WriteLine("Delete data...");
            await unitOfWork.BeginTransactionAsync(ct);
            
            await bookingRepo.DeleteAsync(createdBooking.Id);
            await aptRepo.DeleteAsync(createdApartment.Id);
            await tenantRepo.DeleteAsync(createdTenant.Id);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\nОШИБКА: {ex.Message}");
            System.Console.WriteLine($"Стек: {ex.StackTrace}");
        
            // Откатываем транзакцию при ошибке
            await unitOfWork.RollbackAsync(ct);
        }
        System.Console.WriteLine("\n=== ТЕСТ БД ЗАВЕРШЁН ===");
    }
    
    
    // =============================================================================
    // Тестирование бизнес логики системы
    // =============================================================================
    private static async Task RunBusinessLogicTestsAsync(
        IServiceProvider provider, 
        IMediator mediator, 
        CancellationToken ct)
    {
        Guid? tenantId = null;
        Guid? apartmentId = null;
        Guid? bookingId = null;
        try
        {
            // ШАГ 1: Регистрация пользователя
            System.Console.WriteLine("\nШАГ 1: Регистрация пользователя через Telegram");
           
            var createTenantCmd = new CreateTenantCommand(
                TelegramId: 987654321L,
                FullName: "Иван Петров",
                Phone: "+79001234567",
                Email: "ivan.petrov@test.com",
                PreferredChannel: NotificationChannel.Email  // Чтобы уведомления шли на Email (заглушка)
            );
            tenantId = await mediator.Send(createTenantCmd, ct);
           
            System.Console.WriteLine($"Пользователь создан с ID: {tenantId}");


            // ШАГ 2: Добавление квартиры админом
            System.Console.WriteLine("\nШАГ 2: Добавление квартиры");
            
            var createAptCmd = new CreateApartmentCommand(
                UnitNumber: "101",
                Entrance: 1,
                Floor: 3,
                Capacity: 4,
                BasePricePerNight: 5000,
                Description: "Уютная квартира для тестов"
            );
            apartmentId = await mediator.Send(createAptCmd, ct);
           
            System.Console.WriteLine($"Квартира создана с ID: {apartmentId}");

            
            // ШАГ 3: Поиск квартир пользователем
            System.Console.WriteLine("\nШАГ 3: Поиск квартир");

            // 3.1. Все доступные квартиры (без фильтра по цене)
            var allQuery = new GetApartmentsQuery(new ApartmentFilterCriteria
            {
                Status = ApartmentStatus.Available
            });
            var allApartments = await mediator.Send(allQuery, ct);
            
            System.Console.WriteLine($"Найдено всех доступных квартир: {allApartments.Count()}");
            foreach (var apt in allApartments)
            {
                System.Console.WriteLine($"   - №{apt.UnitNumber}, " +
                                         $"{apt.Floor} этаж, " +
                                         $"{apt.Capacity} чел., " +
                                         $"{apt.BasePricePerNight.Amount} руб/ночь");
            }

            // 3.2. Фильтр по цене: только квартиры до 6000 ₽/ночь
            var cheapQuery = new GetApartmentsQuery(new ApartmentFilterCriteria
            {
                Status = ApartmentStatus.Available,
                MaxPrice = new Money(6000)
            });
            var cheapApartments = await mediator.Send(cheapQuery, ct);
            
            System.Console.WriteLine($"Найдено квартир до 6000 руб: {cheapApartments.Count()}");
            foreach (var apt in cheapApartments)
            {
                System.Console.WriteLine($"   - №{apt.UnitNumber}, " +
                                         $"{apt.Floor} этаж, " +
                                         $"{apt.Capacity} чел., " +
                                         $"{apt.BasePricePerNight.Amount} руб/ночь");
            }

            // 3.3. Фильтр по цене: только дорогие (от 10000 ₽) — должно быть 0
            var expensiveQuery = new GetApartmentsQuery(new ApartmentFilterCriteria
            {
                MinPrice = new Money(10000)
            });
            var expensiveApartments = await mediator.Send(expensiveQuery, ct);
           
            System.Console.WriteLine($"Найдено квартир от 10000 руб: " +
                                     $"{expensiveApartments.Count()} " +
                                     $"(ожидаем 0)");


            // ШАГ 4: Создание брони пользователем
            System.Console.WriteLine("\nШАГ 4: Создание брони");
            
            var createBookingCmd = new CreateBookingCommand(
                TenantId: tenantId.Value,
                ApartmentId: apartmentId.Value,
                CheckInDate: DateTime.Today.AddDays(3),
                CheckOutDate: DateTime.Today.AddDays(7),
                PartySize: 2
            );
            bookingId = await mediator.Send(createBookingCmd, ct);
            
            System.Console.WriteLine($"Бронь создана с ID: {bookingId}");
            

            // ШАГ 5: Админ смотрит список заявок
            System.Console.WriteLine("\nШАГ 5: Админ просматривает заявки (Pending)");
            
            var pendingQuery = new GetPendingBookingsQuery();
            var pendingBookings = await mediator
                .Send(pendingQuery, ct);
            
            System.Console.WriteLine($"Найдено заявок: {pendingBookings.Count()}");
            foreach (var b in pendingBookings)
            {
                System.Console.WriteLine($"   - {b.TenantFullName}, " +
                                         $"кв.№{b.ApartmentUnitNumber}, " +
                                         $"{b.CheckInDate:dd.MM} - {b.CheckOutDate:dd.MM}, " +
                                         $"{b.TotalPrice} руб");
            }


            // ШАГ 6: Админ подтверждает бронь -> должно прийти уведомление на Email
            System.Console.WriteLine("\nШАГ 6: Админ подтверждает бронь -> ожидается EMAIL-уведомление");
            
            var approveCmd = new ApproveBookingCommand(bookingId.Value);
            var approved = await mediator.Send(approveCmd, ct);
            
            System.Console.WriteLine($"Бронь подтверждена: {approved}");
            
            // Небольшая пауза, чтобы MediatR успел обработать события
            await Task.Delay(500, ct);
            
            
            // ШАГ 7: Пользователь смотрит свои брони
            System.Console.WriteLine("\nШАГ 7: Пользователь просматривает свои брони");
            
            var myBookingsQuery = new GetTenantBookingsQuery(tenantId.Value);
            var myBookings = await mediator.Send(myBookingsQuery, ct);
            
            System.Console.WriteLine($"Моих броней: {myBookings.Count()}");
            foreach (var b in myBookings)
            {
                System.Console.WriteLine($"   - кв.№{b.ApartmentUnitNumber}, " +
                                         $"{b.CheckInDate:dd.MM} - {b.CheckOutDate:dd.MM}, " +
                                         $"статус: {b.Status}");
            }

            // ШАГ 8: Пользователь отменяет бронь -> должно прийти уведомление на Email
            System.Console.WriteLine("\nШАГ 8: Пользователь отменяет бронь -> ожидается EMAIL-уведомление");
            
            var cancelCmd = new CancelBookingCommand(bookingId.Value, "Передумал ехать");
            var cancelled = await mediator.Send(cancelCmd, ct);
            
            System.Console.WriteLine($"Бронь отменена: {cancelled}");
            
            await Task.Delay(500, ct);
            
            
            // // ШАГ 9: Массовая рассылка
            // System.Console.WriteLine("\nШАГ 9: Массовая рассылка");
            // var emergencyCmd = new SendEmergencyNotificationCommand(
            //     Message: "Уважаемые гости! Завтра с 10:00 до 12:00 будут проводиться плановые работы. Приносим извинения за неудобства.",
            //     Subject: "⚠️ Плановые работы",
            //     OnlyWithTelegram: false,
            //     OnlyActiveBookings: false
            // );
            // var result = await mediator.Send(emergencyCmd, ct);
            // System.Console.WriteLine($"Рассылка завершена:");
            // System.Console.WriteLine($"   Всего получателей: {result.TotalRecipients}");
            // System.Console.WriteLine($"   Успешно: {result.SuccessfulSends}");
            // System.Console.WriteLine($"   Ошибок: {result.FailedSends}");
            //
            // await Task.Delay(500, ct);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"\nОШИБКА: {ex.Message}");
            System.Console.WriteLine($"Стек: {ex.StackTrace}");
        }
        
        System.Console.WriteLine("\nТЕСТ БИЗНЕС-ЛОГИКИ ЗАВЕРШЁН");
    }
}