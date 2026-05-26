using ApartmentRentalSystem.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ApartmentRentalSystem.Infrastructure;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.ConsoleApp;

public static class Program
{
    // Точка входа: только оркестрация
    public static async Task Main(string[] args)
    {
        // Инициализация конфигурации
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Сборка DI-контейнера
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddInfrastructure(config);
        var provider = services.BuildServiceProvider();

        Console.WriteLine("🚀 Система аренды запущена");
        Console.WriteLine("Выберите режим:\n  1 — Тест БД (CRUD)\n  2 — Демонстрация бота\n  3 — Выйти");
        Console.Write("\nВаш выбор: ");

        var choice = Console.ReadKey(true).KeyChar;
        Console.WriteLine();

        var cts = new CancellationTokenSource();

        try
        {
            switch (choice)
            {
                case '1':
                    await RunDatabaseTestsAsync(provider, cts.Token);
                    break;
                case '2':
                    await RunTelegramBotDemoAsync(provider, cts.Token);
                    break;
                case '3':
                    Console.WriteLine("👋 До свидания!");
                    return;
                default:
                    Console.WriteLine("❌ Неверный выбор");
                    return;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("⚠️ Операция отменена");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка: {ex.Message}");
        }
        finally
        {
            cts.Cancel();
        }
    }
    
    // Тестирование CRUD операций с БД
    private static async Task RunDatabaseTestsAsync(IServiceProvider provider, CancellationToken ct)
    {
        var tenantRepo = provider.GetRequiredService<ITenantRepository>();
        var aptRepo = provider.GetRequiredService<IApartmentRepository>();
        var bookingRepo = provider.GetRequiredService<IBookingRepository>();
        var transactionRepo = provider.GetRequiredService<ITransactionRepository>();
        var notificationRepo = provider.GetRequiredService<INotificationRepository>();

        var suffix = Guid.NewGuid().ToString()[..6];
        var testEmail = $"test_{suffix}@rental.com";
        var unitNumber = $"APT-{suffix}";

        Tenant? createdTenant = null;
        Apartment? createdApartment = null;
        Booking? createdBooking = null;
        Transaction? createdTransaction = null;
        Notification? createdNotification = null;

        try
        {
            // CREATE
            Console.WriteLine("\n📝 1. CREATE: Создание записей...");

            createdTenant = new Tenant(
                email: testEmail,
                passwordHash: "bcrypt_hash_placeholder",
                contact: new ContactInfo("Иван Тестов", testEmail, "+79990001122"),
                userRole: UserRole.User,
                telegramId: 123456789L,
                isBlocked: false);
            await tenantRepo.AddAsync(createdTenant);
            Console.WriteLine($"   ✅ Tenant создан: {createdTenant.Id}");

            createdApartment = new Apartment(
                unitNumber: unitNumber,
                entrance: 2,
                floor: 5,
                capacity: 3,
                basePricePerNight: new Money(4000),
                description: "Тестовая квартира");
            await aptRepo.AddAsync(createdApartment);
            Console.WriteLine($"   ✅ Apartment создан: {createdApartment.Id}");

            createdBooking = new Booking(
                tenantId: createdTenant.Id,
                apartmentId: createdApartment.Id,
                period: new DateRange(DateTime.Today.AddDays(2), DateTime.Today.AddDays(6)),
                totalPrice: new Money(16000),
                deposit: new Money(2000));
            await bookingRepo.AddAsync(createdBooking);
            Console.WriteLine($"   ✅ Booking создан: {createdBooking.Id}");

            createdTransaction = new Transaction(
                bookingId: createdBooking.Id,
                amount: new Money(16000),
                type: TransactionType.Payment);
            await transactionRepo.AddAsync(createdTransaction);
            Console.WriteLine($"   ✅ Transaction создан: {createdTransaction.Id}");

            createdNotification = new Notification(
                userId: createdTenant.Id,
                type: NotificationType.Confirmation,
                channel: NotificationChannel.Email,
                subject: "Бронь подтверждена",
                body: $"Бронь #{createdBooking.Id} создана");
            await notificationRepo.AddAsync(createdNotification);
            Console.WriteLine($"   ✅ Notification создан: {createdNotification.Id}");

            // READ
            Console.WriteLine("\n🔍 2. READ: Чтение данных...");

            var loadedTenant = await tenantRepo.GetByEmailAsync(testEmail);
            Console.WriteLine($"   📌 Tenant: {loadedTenant?.Contact.FullName}");

            var aptById = await aptRepo.GetByIdAsync(createdApartment.Id);
            Console.WriteLine($"   📌 Apartment: {aptById?.UnitNumber} ({aptById?.BasePricePerNight.Amount}₽)");

            var pending = await bookingRepo.GetPendingAsync();
            Console.WriteLine($"   📌 Pending bookings: {pending.Count()}");

            var transactions = await transactionRepo.GetByBookingIdAsync(createdBooking.Id);
            Console.WriteLine($"   📌 Transactions: {transactions.Count()}");

            var notifications = await notificationRepo.GetByUserIdAsync(createdTenant.Id);
            Console.WriteLine($"   📌 Notifications: {notifications.Count()}");

            // UPDATE 
            Console.WriteLine("\n🔄 3. UPDATE: Изменение данных...");

            await bookingRepo.UpdateStatusAsync(createdBooking.Id, BookingStatus.Approved);
            Console.WriteLine($"   📌 Booking статус: {BookingStatus.Approved}");

            createdTransaction.Complete();
            await transactionRepo.UpdateStatusAsync(createdTransaction.Id, createdTransaction.Status);
            Console.WriteLine($"   📌 Transaction статус: {createdTransaction.Status}");

            createdNotification.MarkAsSent();
            await notificationRepo.UpdateStatusAsync(createdNotification.Id, createdNotification.Status);
            Console.WriteLine($"   📌 Notification статус: {createdNotification.Status}");

            // DELETE 
            Console.WriteLine("\n🗑️ 4. DELETE: Очистка...");

            await notificationRepo.DeleteAsync(createdNotification.Id);
            await transactionRepo.DeleteAsync(createdTransaction.Id);
            await bookingRepo.DeleteAsync(createdBooking.Id);
            await aptRepo.DeleteAsync(createdApartment.Id);
            // await tenantRepo.DeleteAsync(createdTenant.Id); // Опционально

            Console.WriteLine("   ✅ Данные удалены");
            Console.WriteLine("\n🏁 === ТЕСТ БД ЗАВЕРШЁН ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ ОШИБКА: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("\n💡 Нажмите Enter для возврата в меню...");
            Console.ReadLine();
        }
    }
    
    // Демонстрация Telegram-бота
    private static async Task RunTelegramBotDemoAsync(IServiceProvider provider, CancellationToken ct)
    {
        var botService = provider.GetRequiredService<ITelegramBotService>();
        var config = provider.GetRequiredService<IConfiguration>();

        Console.WriteLine("\n🤖 Запуск Telegram-бота...");

        // Явный запуск бота
        await botService.StartAsync(ct);

        Console.WriteLine("✅ Бот работает в фоне");
        Console.WriteLine("📱 Откройте Телеграм и напишите боту:");
        Console.WriteLine("   • /start — регистрация");
        Console.WriteLine("   • /help — справка");
        Console.WriteLine("   • /apartments — список квартир");
        Console.WriteLine("\n⌨️ Нажмите 'q' для остановки бота и возврата в меню");

        // Ожидание команды выхода
        while (!ct.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q' || key.KeyChar == 'й')
                {
                    Console.WriteLine("\n🛑 Остановка бота...");
                    break;
                }
            }

            await Task.Delay(100, ct);
        }

        // Явная остановка бота
        await botService.StopAsync(ct);

        Console.WriteLine("✅ Бот остановлен");
        Console.WriteLine("\n💡 Нажмите Enter для возврата в меню...");
        Console.ReadLine();
    }
}