using ApartmentRentalSystem.Application.Commands.Bookings;
using MediatR;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ApartmentRentalSystem.Application.Commands.Tenants;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Application.Queries.Bookings;
using ApartmentRentalSystem.Application.Dto;
using ApartmentRentalSystem.Application.Queries.Apartments;
using ApartmentRentalSystem.Bot.Services;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.ValueObjects;

namespace ApartmentRentalSystem.Bot;

public class TelegramBotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IMediator _mediator;
    private readonly ILogger<TelegramBotService> _logger;
    private readonly ITenantRepository _tenantRepository;
    private readonly UserSessionService _sessionService;
    private readonly IApartmentRepository _apartmentRepository;

    public TelegramBotService(
        ITelegramBotClient botClient,
        IMediator mediator,
        ILogger<TelegramBotService> logger,
        ITenantRepository tenantRepository,
        UserSessionService sessionService,
        IApartmentRepository apartmentRepository)
    {
        _botClient = botClient;
        _mediator = mediator;
        _logger = logger;
        _tenantRepository = tenantRepository;
        _sessionService = sessionService;
        _apartmentRepository = apartmentRepository;
    }

    public async Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("Telegram Bot запускается...");

        var bot = await _botClient.GetMe(ct);
        _logger.LogInformation($"Бот @{bot.Username} запущен");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message, UpdateType.CallbackQuery],
            DropPendingUpdates = true
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            ct
        );
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message when update.Message != null:
                    await HandleMessageAsync(botClient, update.Message, ct);
                    break;
                
                case UpdateType.CallbackQuery when update.CallbackQuery != null:
                    await HandleCallbackQueryAsync(botClient, update.CallbackQuery, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке обновления");
        }
    }

    private async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        if (message.Text == null) return;
        
        var chatId = message.Chat.Id;
        var text = message.Text.Trim().ToLower();
        _logger.LogInformation($"Сообщение от {chatId}: {text}");
        
        var session = _sessionService.GetSession(chatId);
        
        // Обработка состояний FSM
        if (session != null)
        {
            if (session.State == UserState.BookingEnterCheckIn)
            {
                await HandleCheckInDateInputAsync(botClient, message, session, ct);
                return;
            }
    
            if (session.State == UserState.BookingEnterCheckOut)
            {
                await HandleCheckOutDateInputAsync(botClient, message, session, ct);
                return;
            }
        }
        
        switch (text)
        {
            case "/start":
                await HandleStartCommandAsync(botClient, message, ct);
                break;
            
            case "/search":
            case "/поиск":
                await HandleSearchCommandAsync(botClient, message, ct);
                break;
            
            case "/mybookings":
            case "/моиброни":
                await HandleMyBookingsCommandAsync(botClient, message, ct);
                break;
            
            case "/book":
            case "/бронь":
                await HandleBookCommandAsync(botClient, message, ct);
                break;
           
            case "/cancel":
            case "/отмена":
                if (session != null && session.State != UserState.None)
                {
                    session.State = UserState.None;
                    session.ResetBookingData();
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "❌ Действие отменено.",
                        cancellationToken: ct);
                }
                else
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "Нет активного действия для отмены.",
                        cancellationToken: ct);
                }
                break;
            
            case "/help":
            case "/помощь":
                await HandleHelpCommandAsync(botClient, message, ct);
                break;
            
            default:
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❓ Неизвестная команда. Используйте /help для списка команд",
                    cancellationToken: ct);
                break;
        }
    }

    /// <summary>
    /// Обработка нажатий на inline-кнопки
    /// </summary>
    private async Task HandleCallbackQueryAsync(
        ITelegramBotClient botClient, 
        CallbackQuery callbackQuery, 
        CancellationToken ct)
    {
        var chatId = callbackQuery.Message!.Chat.Id;
        var data = callbackQuery.Data;

        try
        {
            // Подтверждаем нажатие
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);

            var session = _sessionService.GetOrCreateSession(chatId);
            
            switch (data)
            {
                case "filter_floor":
                    await ShowFloorFilterMenu(botClient, chatId, ct);
                    break;
                case "filter_capacity":
                    await ShowCapacityFilterMenu(botClient, chatId, ct);
                    break;
                case "filter_price":
                    await ShowPriceFilterMenu(botClient, chatId, ct);
                    break;
                
                // Этаж
                case "floor_1_3":
                    session.SearchFilters!.MinFloor = 1;
                    session.SearchFilters.MaxFloor = 3;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "floor_4_6":
                    session.SearchFilters!.MinFloor = 4;
                    session.SearchFilters.MaxFloor = 6;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "floor_7_plus":
                    session.SearchFilters!.MinFloor = 7;
                    session.SearchFilters.MaxFloor = null;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "floor_any":
                    session.SearchFilters!.MinFloor = null;
                    session.SearchFilters.MaxFloor = null;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                
                // Вместимость
                case "capacity_1_2":
                    session.SearchFilters!.MinCapacity = 1;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "capacity_3_4":
                    session.SearchFilters!.MinCapacity = 3;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "capacity_5_plus":
                    session.SearchFilters!.MinCapacity = 5;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "capacity_any":
                    session.SearchFilters!.MinCapacity = null;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                
                // Цена
                case "price_0_3000":
                    session.SearchFilters!.MinPrice = 0;
                    session.SearchFilters.MaxPrice = 3000;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "price_3000_5000":
                    session.SearchFilters!.MinPrice = 3000;
                    session.SearchFilters.MaxPrice = 5000;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "price_5000_plus":
                    session.SearchFilters!.MinPrice = 5000;
                    session.SearchFilters.MaxPrice = null;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "price_any":
                    session.SearchFilters!.MinPrice = null;
                    session.SearchFilters.MaxPrice = null;
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                
                // Действия
                case "search_apply":
                    await ApplySearchFilters(botClient, chatId, session.SearchFilters!, ct);
                    break;
                case "search_reset":
                    session.SearchFilters!.Reset();
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
                    break;
                case "search_back":
                    await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters!, ct);
                    break;
                
                case string s when s.StartsWith("book_apt_"):
                    var apartmentId = Guid.Parse(s.Replace("book_apt_", ""));
                    await HandleApartmentSelectedAsync(botClient, chatId, session, apartmentId, ct);
                    break;

                case "book_confirm":
                    await HandleBookingConfirmationAsync(botClient, chatId, session, ct);
                    break;
                
                case "book_cancel":
                    session.State = UserState.None;
                    session.ResetBookingData();
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "❌ Бронирование отменено.",
                        cancellationToken: ct);
                    break;
                
                case string s when s.StartsWith("cancel_booking_"):
                    var bookingId = Guid.Parse(s.Replace("cancel_booking_", ""));
                    await HandleCancelBookingAsync(botClient, chatId, session, bookingId, ct);
                    break;

                case "search_from_bookings":
                    session.State = UserState.SearchingApartments;
                    await ShowSearchFiltersMenu(botClient, chatId, new SearchFilters(), ct);
                    break;
                
                case string s when s.StartsWith("confirm_cancel_"):
                    var bookingIdToCancel = Guid.Parse(s.Replace("confirm_cancel_", ""));
                    await ExecuteCancelBookingAsync(botClient, chatId, session, bookingIdToCancel, ct);
                    break;

                case "cancel_no":
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: "✅ Отмена отменена. Ваша бронь сохранена.",
                        cancellationToken: ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке callback query: {Data}", data);
        }
    }

    private async Task HandleStartCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var telegramId = message.Chat.Id;   // Telegram ID = Chat ID для личных сообщений

        var firstName = message.Chat.FirstName ?? "пользователь";
        var lastName = message.Chat.LastName ?? "";
        var fullName = $"{firstName} {lastName}".Trim();
        
        try
        {
            // 1. Проверяем, есть ли уже пользователь с таким Telegram ID
            var existingTenant = await _tenantRepository.GetByTelegramIdAsync(telegramId);
            
            if (existingTenant != null) // Пользователь уже зарегистрирован
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"👋 С возвращением, {firstName}!\n\n" +
                          $"Вы уже зарегистрированы в системе.\n\n" +
                          $"Доступные команды:\n" +
                          $"/search - Поиск квартир\n" +
                          $"/mybookings - Мои брони\n" +
                          $"/help - Помощь",
                    cancellationToken: ct);
                return;
            }
            
            // 2. Создаём нового пользователя
            var command = new CreateTenantCommand(
                TelegramId: telegramId,
                FullName: fullName,
                Phone: "",      // Пока пустой, пользователь сможет добавить позже
                Email: null,    // Запросим отдельно
                PreferredChannel: NotificationChannel.Telegram // По умолчанию Telegram
            );
            var tenantId = await _mediator.Send(command, ct);
            _logger.LogInformation($"Пользователь зарегистрирован: {fullName} (ID: {tenantId}, Telegram: {telegramId})");

            // 3. Отправляем приветственное сообщение
            await botClient.SendMessage(
                chatId: chatId,
                text: $"👋 Добро пожаловать, {firstName}!\n\n" +
                      $"✅ Вы успешно зарегистрированы!\n\n" +
                      $"Я бот для аренды квартир.\n\n" +
                      $"Доступные команды:\n" +
                      $"/search - Поиск квартир\n" +
                      $"/mybookings - Мои брони\n" +
                      $"/help - Помощь",
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке /start");
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка. Попробуйте позже.",
                cancellationToken: ct);
        }
    }

    private async Task HandleSearchCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        try
        {
            // Создаём или получаем сессию
            var session = _sessionService.GetOrCreateSession(chatId);
            session.State = UserState.SearchingApartments;
            session.SearchFilters ??= new SearchFilters();
            session.SearchFilters.Reset(); // Сбрасываем предыдущие фильтры

            // Показываем меню фильтров
            await ShowSearchFiltersMenu(botClient, chatId, session.SearchFilters, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инициализации поиска квартир");
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка. Попробуйте позже.",
                cancellationToken: ct);
        }
    }

    private async Task HandleMyBookingsCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
{
    var chatId = message.Chat.Id;

    try
    {
        var tenant = await _tenantRepository.GetByTelegramIdAsync(chatId);
        
        if (tenant == null)
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Вы не зарегистрированы.\n\nИспользуйте /start для регистрации.",
                cancellationToken: ct);
            return;
        }

        var query = new GetTenantBookingsQuery(tenant.Id);
        var bookings = (await _mediator.Send(query, ct)).ToList();

        if (!bookings.Any())
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "📋 У вас пока нет броней.\n\nИспользуйте /search для поиска доступных квартир.",
                cancellationToken: ct);
            return;
        }

        var response = "📋 <b>Ваши брони:</b>\n\n";
        
        var keyboardButtons = new List<InlineKeyboardButton[]>();
        
        for (int i = 0; i < bookings.Count; i++)
        {
            var booking = bookings[i];
            var statusEmoji = GetStatusEmoji(booking.Status);
            var statusText = GetStatusText(booking.Status);
            
            response += $"{i + 1}. {statusEmoji} <b>Квартира №{booking.ApartmentUnitNumber}</b>\n" +
                       $"📅 {booking.CheckInDate:dd.MM.yyyy} — {booking.CheckOutDate:dd.MM.yyyy}\n" +
                       $"💰 {booking.TotalPrice:F2} ₽\n" +
                       $"🏷️ Статус: {statusText}\n\n";

            // Добавляем кнопку отмены только для Pending броней
            if (booking.Status.ToLower() == "pending")
            {
                keyboardButtons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"❌ Отменить бронь #{booking.BookingId.ToString()[..8]}", 
                        $"cancel_booking_{booking.BookingId}")
                });
            }
        }

        keyboardButtons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(" Поиск квартир", "search_from_bookings")
        });

        var keyboard = new InlineKeyboardMarkup(keyboardButtons);

        await botClient.SendMessage(
            chatId: chatId,
            text: response,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Ошибка при получении броней для пользователя {ChatId}", chatId);
        await botClient.SendMessage(
            chatId: chatId,
            text: "❌ Произошла ошибка при получении списка броней. Попробуйте позже.",
            cancellationToken: ct);
    }
}    
    
    private async Task HandleBookCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        try
        {
            // Проверяем, зарегистрирован ли пользователь
            var tenant = await _tenantRepository.GetByTelegramIdAsync(chatId);
            if (tenant == null)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Вы не зарегистрированы.\n\nИспользуйте /start для регистрации.",
                    cancellationToken: ct);
                return;
            }

            // Получаем все доступные квартиры
            var query = new GetApartmentsQuery(new ApartmentFilterCriteria
            {
                Status = ApartmentStatus.Available
            });
            var apartments = (await _mediator.Send(query, ct)).ToList();

            if (apartments.Count == 0)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: " Сейчас нет доступных квартир для бронирования.\n\nПопробуйте позже.",
                    cancellationToken: ct);
                return;
            }

            // Сохраняем состояние
            var session = _sessionService.GetOrCreateSession(chatId);
            session.TenantId = tenant.Id;
            session.State = UserState.BookingSelectApartment;
            session.ResetBookingData();

            // Показываем список квартир
            await ShowApartmentSelectionMenu(botClient, chatId, apartments, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инициализации бронирования");
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка. Попробуйте позже.",
                cancellationToken: ct);
        }
    }
    
    private async Task HandleHelpCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;

        await botClient.SendMessage(
            chatId: chatId,
            text: "📖 Доступные команды:\n\n" +
                  "/start - Начать работу с ботом\n" +
                  "/search - Поиск доступных квартир\n" +
                  "/book - Создать бронь\n" +
                  "/mybookings - Посмотреть мои брони\n" +
                  "/help - Показать эту справку",
            cancellationToken: ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception, "Ошибка в Telegram Bot");
        return Task.CompletedTask;
    }
    
    
    // - - Вспомогательные методы - -
    
    /// <summary>
    /// Получает эмодзи для статуса брони
    /// </summary>
    private static string GetStatusEmoji(string status)
    {
        return status.ToLower() switch
        {
            "pending" => "⏳",    // Ожидает подтверждения
            "approved" => "✅",   // Подтверждена
            "cancelled" => "❌",  // Отменена
            "completed" => "🏁",  // Завершена
            _ => "❓"
        };
    }

    /// <summary>
    /// Получает читаемое название статуса
    /// </summary>
    private static string GetStatusText(string status)
    {
        return status.ToLower() switch
        {
            "pending" => "Ожидает подтверждения",
            "approved" => "Подтверждена",
            "cancelled" => "Отменена",
            "completed" => "Завершена",
            _ => status
        };
    }
    
    /// <summary>
    /// Показывает меню с кнопками фильтров
    /// </summary>
    private async Task ShowSearchFiltersMenu(
        ITelegramBotClient botClient, 
        long chatId, 
        SearchFilters filters,
        CancellationToken ct)
    {
        var text = "🔍 <b>Поиск квартир</b>\n\n";
        text += "Выберите фильтры или нажмите \"Показать результаты\":\n\n";
    
        // Показываем текущие выбранные фильтры
        text += "<b>Текущие фильтры:</b>\n";
        text += $"📍 Этаж: {FormatFloorFilter(filters)}\n";
        text += $"👥 Вместимость: {FormatCapacityFilter(filters)}\n";
        text += $"💰 Цена: {FormatPriceFilter(filters)}\n";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📍 Этаж", "filter_floor"),
                InlineKeyboardButton.WithCallbackData("👥 Вместимость", "filter_capacity")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("💰 Цена", "filter_price")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("✅ Показать результаты", "search_apply")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Сбросить фильтры", "search_reset")
            }
        });

        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    
    private static string FormatFloorFilter(SearchFilters filters)
    {
        if (filters.MinFloor.HasValue && filters.MaxFloor.HasValue)
            return $"{filters.MinFloor}-{filters.MaxFloor} этаж";
        if (filters.MinFloor.HasValue)
            return $"от {filters.MinFloor} этажа";
        if (filters.MaxFloor.HasValue)
            return $"до {filters.MaxFloor} этажа";
        return "любой";
    }

    private static string FormatCapacityFilter(SearchFilters filters)
    {
        return filters.MinCapacity.HasValue ? $"от {filters.MinCapacity} чел." : "любая";
    }

    private static string FormatPriceFilter(SearchFilters filters)
    {
        if (filters.MinPrice.HasValue && filters.MaxPrice.HasValue)
            return $"{filters.MinPrice:F0}-{filters.MaxPrice:F0} ₽";
        if (filters.MinPrice.HasValue)
            return $"от {filters.MinPrice:F0} ₽";
        if (filters.MaxPrice.HasValue)
            return $"до {filters.MaxPrice:F0} ₽";
        return "любая";
    }
    
    
    /// <summary>
    /// Показывает меню выбора этажа
    /// </summary>
    private async Task ShowFloorFilterMenu(ITelegramBotClient botClient, long chatId, CancellationToken ct)
    {
        var text = "📍 <b>Выберите диапазон этажей:</b>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("1-3 этаж", "floor_1_3"),
                InlineKeyboardButton.WithCallbackData("4-6 этаж", "floor_4_6")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("7+ этаж", "floor_7_plus")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Любой этаж", "floor_any")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад", "search_back")
            }
        });
        
        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    /// <summary>
    /// Показывает меню выбора вместимости
    /// </summary>
    private async Task ShowCapacityFilterMenu(ITelegramBotClient botClient, long chatId, CancellationToken ct)
    {
        var text = "👥 <b>Выберите минимальную вместимость:</b>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("1-2 чел.", "capacity_1_2"),
                InlineKeyboardButton.WithCallbackData("3-4 чел.", "capacity_3_4")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("5+ чел.", "capacity_5_plus")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Любая", "capacity_any")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад", "search_back")
            }
        });

        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    
    
    /// <summary>
    /// Показывает меню выбора цены
    /// </summary>
    private async Task ShowPriceFilterMenu(ITelegramBotClient botClient, long chatId, CancellationToken ct)
    {
        var text = "💰 <b>Выберите диапазон цен за ночь:</b>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("до 3000 ₽", "price_0_3000"),
                InlineKeyboardButton.WithCallbackData("3000-5000 ₽", "price_3000_5000")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("от 5000 ₽", "price_5000_plus")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Любая цена", "price_any")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("◀️ Назад", "search_back")
            }
        });
        
        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    /// <summary>
    /// Применяет фильтры и показывает результаты
    /// </summary>
    private async Task ApplySearchFilters(
        ITelegramBotClient botClient,
        long chatId,
        SearchFilters filters,
        CancellationToken ct)
    {
        try
        {
            // Формируем критерии фильтрации
            var criteria = new ApartmentFilterCriteria
            {
                Status = filters.Status ?? ApartmentStatus.Available
            };
            if (filters.MinFloor.HasValue)
                criteria.MinFloor = filters.MinFloor.Value;
            if (filters.MaxFloor.HasValue)
                criteria.MaxFloor = filters.MaxFloor.Value;
            if (filters.MinCapacity.HasValue)
                criteria.MinCapacity = filters.MinCapacity.Value;
            if (filters.MinPrice.HasValue)
                criteria.MinPrice = new Money(filters.MinPrice.Value);
            if (filters.MaxPrice.HasValue)
                criteria.MaxPrice = new Money(filters.MaxPrice.Value);

            // Выполняем запрос
            var query = new GetApartmentsQuery(criteria);
            var apartments = (await _mediator.Send(query, ct)).ToList();

            // Сбрасываем состояние
            var session = _sessionService.GetOrCreateSession(chatId);
            session.State = UserState.None;

            // Показываем результаты
            if (!apartments.Any())
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "🏠 По вашим фильтрам квартир не найдено.\n\n" +
                          "Попробуйте изменить фильтры или используйте /search заново.",
                    cancellationToken: ct);
                return;
            }

            // Форматируем список
            const int maxApartments = 10;
            var response = $"🏠 <b>Найдено квартир: {apartments.Count}</b>\n\n";

            var apartmentsToShow = apartments.Take(maxApartments).ToList();

            for (int i = 0; i < apartmentsToShow.Count; i++)
            {
                var apt = apartmentsToShow[i];

                response += $"{i + 1}. <b>Квартира №{apt.UnitNumber}</b>\n" +
                            $"📍 Подъезд: {apt.Entrance}, Этаж: {apt.Floor}\n" +
                            $"👥 Вместимость: {apt.Capacity} чел.\n" +
                            $"💰 Цена: {apt.BasePricePerNight.Amount:F2} ₽/ночь\n";

                if (!string.IsNullOrWhiteSpace(apt.Description))
                {
                    var shortDesc = apt.Description.Length > 100
                        ? apt.Description[..100] + "..."
                        : apt.Description;
                    response += $"📝 {shortDesc}\n";
                }

                response += "\n";
            }

            if (apartments.Count > maxApartments)
            {
                response += $"<i>... и ещё {apartments.Count - maxApartments} квартир.</i>\n\n";
            }

            response += "Чтобы забронировать квартиру, используйте /book";

            await botClient.SendMessage(
                chatId: chatId,
                text: response,
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при применении фильтров поиска");
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка при поиске. Попробуйте позже.",
                cancellationToken: ct);
        }
    }
    
    /// <summary>
    /// Показывает список квартир для выбора
    /// </summary>
    private async Task ShowApartmentSelectionMenu(
        ITelegramBotClient botClient, 
        long chatId, 
        List<Apartment> apartments,
        CancellationToken ct)
    {
        var text = "🏠 <b>Выберите квартиру для бронирования:</b>\n\n";

        // Создаём inline-кнопки (максимум 10 квартир)
        var buttons = new List<InlineKeyboardButton[]>();
    
        foreach (var apt in apartments.Take(10))
        {
            var buttonText = $"№{apt.UnitNumber} | {apt.Floor} эт. | {apt.Capacity} чел. | {apt.BasePricePerNight.Amount:F0}₽";
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(buttonText, $"book_apt_{apt.Id}")
            });
        }
        // Кнопка отмены
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("❌ Отмена", "book_cancel")
        });

        var keyboard = new InlineKeyboardMarkup(buttons);

        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
    
    /// <summary>
    /// Обработка выбора квартиры
    /// </summary>
    private async Task HandleApartmentSelectedAsync(
        ITelegramBotClient botClient, 
        long chatId, 
        UserSession session,
        Guid apartmentId,
        CancellationToken ct)
    {
        session.SelectedApartmentId = apartmentId;
        session.State = UserState.BookingEnterCheckIn;

        await botClient.SendMessage(
            chatId: chatId,
            text: "📅 <b>Введите дату заезда</b> в формате ДД.ММ.ГГГГ\n\n" +
                  "Например: 25.06.2026\n\n" +
                  "Для отмены введите /cancel",
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }
    
    /// <summary>
    /// Обработка ввода даты заезда
    /// </summary>
    private async Task HandleCheckInDateInputAsync(
        ITelegramBotClient botClient, 
        Message message, 
        UserSession session,
        CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var text = message.Text.Trim();

        if (DateTime.TryParseExact(text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var checkInDate))
        {
            // Проверяем, что дата не в прошлом
            if (checkInDate.Date < DateTime.Today)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Дата заезда не может быть в прошлом.\n\nВведите дату ещё раз:",
                    cancellationToken: ct);
                return;
            }

            session.CheckInDate = checkInDate;
            session.State = UserState.BookingEnterCheckOut;

            await botClient.SendMessage(
                chatId: chatId,
                text: " <b>Введите дату выезда</b> в формате ДД.ММ.ГГГГ\n\n" +
                      "Например: 30.06.2026\n\n" +
                      "Для отмены введите /cancel",
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: " Неверный формат даты. Используйте ДД.ММ.ГГГГ\n\n" +
                      "Например: 25.06.2026\n\n" +
                      "Попробуйте ещё раз:",
                cancellationToken: ct);
        }
    }

    /// <summary>
    /// Обработка ввода даты выезда
    /// </summary>
    private async Task HandleCheckOutDateInputAsync(
        ITelegramBotClient botClient, 
        Message message, 
        UserSession session,
        CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var text = message.Text.Trim();

        if (DateTime.TryParseExact(text, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var checkOutDate))
        {
            // Проверяем, что дата выезда после даты заезда
            if (checkOutDate.Date <= session.CheckInDate!.Value.Date)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Дата выезда должна быть позже даты заезда.\n\nВведите дату ещё раз:",
                    cancellationToken: ct);
                return;
            }

            session.CheckOutDate = checkOutDate;
            session.State = UserState.BookingConfirm;

            // Получаем информацию о квартире
            var apartment = await _apartmentRepository.GetByIdAsync(session.SelectedApartmentId!.Value);
            if (apartment == null)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Квартира не найдена. Попробуйте ещё раз.",
                    cancellationToken: ct);
                session.State = UserState.None;
                return;
            }

            // Рассчитываем стоимость
            var nights = (checkOutDate.Date - session.CheckInDate.Value.Date).Days;
            var totalPrice = apartment.BasePricePerNight.Amount * nights;

            // Показываем детали и запрашиваем подтверждение
            var text_message = $"📋 <b>Детали бронирования:</b>\n\n" +
                              $"🏠 Квартира №{apartment.UnitNumber}\n" +
                              $" Подъезд: {apartment.Entrance}, Этаж: {apartment.Floor}\n" +
                              $"👥 Вместимость: {apartment.Capacity} чел.\n\n" +
                              $"📅 Заезд: {session.CheckInDate:dd.MM.yyyy}\n" +
                              $"📅 Выезд: {session.CheckOutDate:dd.MM.yyyy}\n" +
                              $"🌙 Количество ночей: {nights}\n\n" +
                              $"💰 Стоимость за ночь: {apartment.BasePricePerNight.Amount:F2} ₽\n" +
                              $"💵 <b>Итого: {totalPrice:F2} ₽</b>\n\n" +
                              $"Подтвердить бронирование?";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Подтвердить", "book_confirm"),
                    InlineKeyboardButton.WithCallbackData("❌ Отмена", "book_cancel")
                }
            });

            await botClient.SendMessage(
                chatId: chatId,
                text: text_message,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
        else
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Неверный формат даты. Используйте ДД.ММ.ГГГГ\n\n" +
                      "Например: 30.06.2026\n\n" +
                      "Попробуйте ещё раз:",
                cancellationToken: ct);
        }
    }
    
    /// <summary>
    /// Обработка подтверждения брони
    /// </summary>
    private async Task HandleBookingConfirmationAsync(
        ITelegramBotClient botClient, 
        long chatId, 
        UserSession session,
        CancellationToken ct)
    {
        try
        {
            // Создаём команду
            var command = new CreateBookingCommand(
                TenantId: session.TenantId!.Value,
                ApartmentId: session.SelectedApartmentId!.Value,
                CheckInDate: session.CheckInDate!.Value,
                CheckOutDate: session.CheckOutDate!.Value,
                PartySize: session.PartySize
            );

            // Выполняем команду
            var bookingId = await _mediator.Send(command, ct);

            // Сбрасываем состояние
            session.State = UserState.None;
            session.ResetBookingData();

            await botClient.SendMessage(
                chatId: chatId,
                text: $"✅ <b>Бронь успешно создана!</b>\n\n" +
                      $"ID брони: {bookingId}\n\n" +
                      $"Администратор рассмотрит вашу заявку и свяжется с вами.\n\n" +
                      $"Используйте /mybookings для просмотра ваших броней.",
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании брони");
        
            session.State = UserState.None;
            session.ResetBookingData();

            await botClient.SendMessage(
                chatId: chatId,
                text: $" Ошибка при создании брони:\n\n{ex.Message}\n\nПопробуйте ещё раз.",
                cancellationToken: ct);
        }
    }
    
    /// <summary>
    /// Обработка отмены брони пользователем
    /// </summary>
    private async Task HandleCancelBookingAsync(
        ITelegramBotClient botClient, 
        long chatId, 
        UserSession session,
        Guid bookingId,
        CancellationToken ct)
    {
        try
        {
            // Получаем информацию о броне
            var query = new GetBookingDetailsQuery(bookingId);
            var booking = await _mediator.Send(query, ct);

            if (booking == null)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Бронь не найдена.",
                    cancellationToken: ct);
                return;
            }

            // Проверяем, что это бронь текущего пользователя
            var tenant = await _tenantRepository.GetByTelegramIdAsync(chatId);
            if (booking.TenantId != tenant?.Id)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ У вас нет доступа к этой брони.",
                    cancellationToken: ct);
                return;
            }

            // Проверяем статус (можно отменить только Pending)
            if (booking.Status.ToLower() != "pending")
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"❌ Нельзя отменить бронь со статусом \"{GetStatusText(booking.Status)}\".\n\n" +
                          "Отменить можно только брони в статусе \"Ожидает подтверждения\".",
                    cancellationToken: ct);
                return;
            }

            // Показываем подтверждение
            var confirmText = $"⚠️ <b>Подтвердите отмену брони</b>\n\n" +
                             $"Квартира №{booking.ApartmentUnitNumber}\n" +
                             $"📅 {booking.CheckInDate:dd.MM.yyyy} — {booking.CheckOutDate:dd.MM.yyyy}\n" +
                             $"💰 {booking.TotalPrice:F2} ₽\n\n" +
                             $"Вы уверены, что хотите отменить эту бронь?";

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("✅ Да, отменить", $"confirm_cancel_{bookingId}"),
                    InlineKeyboardButton.WithCallbackData("❌ Нет", "cancel_no")
                }
            });

            await botClient.SendMessage(
                chatId: chatId,
                text: confirmText,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при инициализации отмены брони {BookingId}", bookingId);
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка. Попробуйте позже.",
                cancellationToken: ct);
        }
    }
    /// <summary>
    /// Выполняет отмену брони
    /// </summary>
    private async Task ExecuteCancelBookingAsync(
        ITelegramBotClient botClient, 
        long chatId, 
        UserSession session,
        Guid bookingId,
        CancellationToken ct)
    {
        try
        {
            // Используем CancelBookingCommand
            var command = new CancelBookingCommand(bookingId, "Отменено пользователем через бота");
            var result = await _mediator.Send(command, ct);

            if (result)
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"✅ <b>Бронь отменена!</b>\n\n" +
                          $"ID брони: {bookingId}\n\n" +
                          $"Используйте /mybookings для просмотра остальных броней.",
                    parseMode: ParseMode.Html,
                    cancellationToken: ct);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Не удалось отменить бронь. Возможно, она уже была обработана администратором.",
                    cancellationToken: ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отмене брони {BookingId}", bookingId);
            await botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка при отмене брони. Попробуйте позже.",
                cancellationToken: ct);
        }
    }
}