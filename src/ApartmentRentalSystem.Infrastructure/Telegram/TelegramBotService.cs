using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Application.Services;
using ApartmentRentalSystem.Infrastructure.Telegram;

namespace ApartmentRentalSystem.Infrastructure.Telegram;

public class TelegramBotService : ITelegramBotService
{
    private readonly TelegramBotClient _bot;
    private readonly IConfiguration _config;
    private readonly ITelegramUserService _userService;
    private readonly IApartmentService _apartmentService;
    private readonly IBookingService _bookingService;

    private readonly ConcurrentDictionary<long, BotState> _userStates = new();
    private readonly ConcurrentDictionary<Guid, long> _tenantToChatId = new();
    private readonly long[] _adminChatIds;

    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public TelegramBotService(
        IConfiguration config, 
        ITelegramUserService userService, 
        IApartmentService apartmentService, 
        IBookingService bookingService)
    {
        _config = config;
        _userService = userService;
        _apartmentService = apartmentService;
        _bookingService = bookingService;
        
        var adminIds = _config["Telegram:AdminChatIds"]?.Split(',') ?? Array.Empty<string>();
        _adminChatIds = adminIds
            .Select(s => long.TryParse(s.Trim(), out var id) ? id : 0)
            .Where(id => id != 0)
            .ToArray();
        
        var token = _config["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Не найден Telegram:BotToken в конфигурации");
            
        _bot = new TelegramBotClient(token);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (_isRunning) return;
        
        var me = await _bot.GetMe(ct);
        System.Console.WriteLine($"Бот подключен: @{me.Username}");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        
        _bot.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
            },
            _cts.Token
        );
        
        _isRunning = true;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (!_isRunning) return;
        
        System.Console.WriteLine("Остановка бота");
        
        if (_cts != null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        _isRunning = false;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text is string text)
        {
            var chatId = update.Message.Chat.Id;
            var username = update.Message.From?.Username ?? "User";
            System.Console.WriteLine($"Сообщение от {username} ({chatId}): {text}");

            switch (text.ToLower())
            {
                case "/start": await HandleStartCommand(chatId, username, ct); break;
                case "/apartments": case "квартиры": await HandleListApartments(chatId, ct); break;
                case "/pending": case "заявки": await HandlePendingBookings(chatId, ct); break;
                case "/book": case "забронировать": await HandleStartBooking(chatId, ct); break;
                case "/mybookings": case "мои брони": await HandleMyBookings(chatId, ct); break;
                case "/cancel": case "отменить бронь": await HandleCancelBooking(chatId, ct); break;
                case "/help": await ShowHelp(chatId, ct); break;
                default: await HandleStateInput(chatId, text, ct); break;
            }
            return;
        }

        if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data is string callbackData)
        {
            var callbackChatId = update.CallbackQuery.Message?.Chat.Id ?? 0;
            await HandleCallbackAsync(callbackChatId, callbackData, ct);
        }
    }

    private async Task HandleStartCommand(long chatId, string username, CancellationToken ct)
    {
        var result = await _userService.GetOrCreateUserAsync(chatId, username, ct);
        
        if (result.IsSuccess)
        {
            if (result.User?.Id is Guid tenantId)
            {
                _tenantToChatId[tenantId] = chatId;
            }

            var message = result.IsNewRegistration
                ? $"Регистрация успешна! Привет, {username}!"
                : $"С возвращением, {result.User?.FullName ?? username}!";

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Смотреть квартиры", "menu_apartments"),
                    InlineKeyboardButton.WithCallbackData("Мои брони", "menu_mybookings")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Помощь", "menu_help")
                }
            });
            
            await _bot.SendMessage(chatId, message, replyMarkup: markup, cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId, $"Ошибка: {result.ErrorMessage}", cancellationToken: ct);
        }
    }

    private async Task HandleListApartments(long chatId, CancellationToken ct)
    {
        try
        {
            var apartments = await _apartmentService.GetAvailableApartmentsAsync(ct);
            var list = apartments.ToList();

            if (!list.Any())
            {
                await _bot.SendMessage(chatId, "Сейчас нет свободных квартир", cancellationToken: ct);
                return;
            }

            var sb = new StringBuilder("<b>Доступные квартиры:</b>\n\n");
            var buttons = new List<InlineKeyboardButton[]>();

            foreach (var apt in list)
            {
                sb.AppendLine($"<b>{apt.UnitNumber}</b>");
                sb.AppendLine($"Этаж: {apt.Floor}, Количество людей: {apt.Capacity}");
                sb.AppendLine($"Стоимость: {apt.Price:N0} ₽/ночь");
                sb.AppendLine($"Статус: {apt.Status}");

                buttons.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        $"Выбрать {apt.UnitNumber}", 
                        $"apt_select_{apt.Id}")
                });
            }

            await _bot.SendMessage(
                chatId, 
                sb.ToString(), 
                replyMarkup: new InlineKeyboardMarkup(buttons),
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await _bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
        }
    }

    private async Task HandleMyBookings(long chatId, CancellationToken ct)
    {
        var userResult = await _userService.GetOrCreateUserAsync(chatId, "temp", ct);
        if (!userResult.IsSuccess || userResult.User?.Id is not Guid tenantId)
        {
            await _bot.SendMessage(chatId, "Ошибка: пользователь не найден. Попробуйте /start", cancellationToken: ct);
            return;
        }

        try
        {
            var bookings = await _bookingService.GetByTenantIdAsync(tenantId, ct);
            var list = bookings.ToList();

            if (!list.Any())
            {
                await _bot.SendMessage(chatId, "У вас пока нет активных броней.\nНажмите /apartments, чтобы выбрать квартиру!", cancellationToken: ct);
                return;
            }

            var sb = new StringBuilder("<b>Ваши брони:</b>\n\n");

            foreach (var b in list)
            {
                var shortId = b.Id.ToString("N")[..8];
                var nights = (b.Period.End - b.Period.Start).Days;

                sb.AppendLine($"<b>#{shortId}</b>");
                sb.AppendLine($"{b.Period.Start:dd.MM.yyyy} — {b.Period.End:dd.MM.yyyy} ({nights} ночей)");
                sb.AppendLine($"{b.TotalPrice.Amount:N0} ₽");
                sb.AppendLine($"Статус: {GetStatusName(b.Status)}");
            }

            var markup = new InlineKeyboardMarkup(new[]
            {
                InlineKeyboardButton.WithCallbackData("Выбрать ещё", "menu_apartments")
            });

            await _bot.SendMessage(
                chatId, 
                sb.ToString(), 
                replyMarkup: markup,
                parseMode: ParseMode.Html,
                cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await _bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
        }
    }

    private string GetStatusName(BookingStatus status) => status switch
    {
        BookingStatus.Pending => "Ожидает подтверждения",
        BookingStatus.Approved => "Подтверждена",
        BookingStatus.Rejected => "Отклонена",
        BookingStatus.Cancelled => "Отменена",
        BookingStatus.Completed => "Завершена",
        _ => status.ToString()
    };

    // Отмена брони: показываем список активных броней с кнопками
    private async Task HandleCancelBooking(long chatId, CancellationToken ct)
    {
        var userResult = await _userService.GetOrCreateUserAsync(chatId, "temp", ct);
        if (!userResult.IsSuccess || userResult.User?.Id is not Guid tenantId)
        {
            await _bot.SendMessage(chatId, "Сначала пройдите регистрацию (/start)", cancellationToken: ct);
            return;
        }

        var allBookings = await _bookingService.GetByTenantIdAsync(tenantId, ct);
        var cancellable = allBookings
            .Where(b => b.Status is BookingStatus.Pending or BookingStatus.Approved)
            .ToList();

        if (!cancellable.Any())
        {
            await _bot.SendMessage(chatId, "Нет активных броней для отмены", cancellationToken: ct);
            return;
        }

        var sb = new StringBuilder("<b>Выберите бронь для отмены:</b>\n\n");
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var b in cancellable)
        {
            var shortId = b.Id.ToString("N")[..8];
            sb.AppendLine($"#{shortId} | {b.Period.Start:dd.MM}–{b.Period.End:dd.MM}");
            
            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData($"Отменить #{shortId}", $"cancel_booking_{b.Id}")
            });
        }

        await _bot.SendMessage(
            chatId, 
            sb.ToString(), 
            replyMarkup: new InlineKeyboardMarkup(buttons),
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }

    private async Task ShowHelp(long chatId, CancellationToken ct)
    {
        var helpText = "<b>Доступные команды:</b>\n" +
                       "/start - Регистрация\n" +
                       "/apartments - Список квартир\n" +
                       "/pending - Заявки (только админ)\n" +
                       "/cancel - Отменить бронь\n" +
                       "/help - Эта справка";
        await _bot.SendMessage(chatId, helpText, parseMode: ParseMode.Html, cancellationToken: ct);
    }

    private async Task HandleCallbackAsync(long chatId, string data, CancellationToken ct)
    {
        // Кнопки главного меню
        if (data == "menu_apartments")
        {
            await HandleListApartments(chatId, ct);
            return;
        }
        if (data == "menu_mybookings")
        {
            await HandleMyBookings(chatId, ct);
            return;
        }
        if (data == "menu_help")
        {
            await ShowHelp(chatId, ct);
            return;
        }

        // Выбор квартиры пользователем
        if (data.StartsWith("apt_select_"))
        {
            var guidStr = data.Replace("apt_select_", "");
            if (Guid.TryParse(guidStr, out var apartmentId))
                await HandleApartmentSelected(chatId, apartmentId, ct);
            else
                await _bot.SendMessage(chatId, "Ошибка: неверный ID квартиры", cancellationToken: ct);
            return;
        }

        // 🔹 Отмена брони пользователем
        if (data.StartsWith("cancel_booking_"))
        {
            var idStr = data.Replace("cancel_booking_", "");
            if (Guid.TryParse(idStr, out var bookingId))
                await HandleCancelBookingAction(chatId, bookingId, ct);
            return;
        }

        // Админ: одобрение
        if (data.StartsWith("admin_approve_"))
        {
            if (!IsAdmin(chatId)) return;
            var idStr = data.Replace("admin_approve_", "");
            if (Guid.TryParse(idStr, out var bookingId))
                await HandleAdminApprove(chatId, bookingId, ct);
            return;
        }
        
        // Админ: отклонение
        if (data.StartsWith("admin_reject_"))
        {
            if (!IsAdmin(chatId)) return;
            var idStr = data.Replace("admin_reject_", "");
            if (Guid.TryParse(idStr, out var bookingId))
                await HandleAdminReject(chatId, bookingId, ct);
            return;
        }
    }

    // 🔹 Обработка нажатия кнопки "Отменить"
    private async Task HandleCancelBookingAction(long chatId, Guid bookingId, CancellationToken ct)
    {
        var userResult = await _userService.GetOrCreateUserAsync(chatId, "temp", ct);
        if (!userResult.IsSuccess || userResult.User?.Id is not Guid tenantId) return;

        try
        {
            await _bookingService.CancelBookingAsync(bookingId, tenantId, ct);
            await _bot.SendMessage(chatId, "Бронь успешно отменена.", cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await _bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
        }
    }

    private async Task HandleApartmentSelected(long chatId, Guid apartmentId, CancellationToken ct)
    {
        var state = _userStates.GetOrAdd(chatId, _ => new BotState());
        state.SelectedApartmentId = apartmentId;
        state.CurrentStep = BookingStep.SelectingDates;

        var message = $"Вы выбрали квартиру #{apartmentId.ToString()[..8]}...\n\n" +
                      $"Введите даты бронирования в формате:\n" +
                      "`ДД.ММ - ДД.ММ`\n" +
                      $"Например: `10.05 - 15.05`";

        await _bot.SendMessage(chatId, message, parseMode: ParseMode.Markdown, cancellationToken: ct);
    }

    private async Task HandleDateInputAsync(long chatId, string text, BotState state, CancellationToken ct)
    {
        var match = Regex.Match(text, @"(\d{2})\.(\d{2})\s*-\s*(\d{2})\.(\d{2})");
        
        if (!match.Success)
        {
            await _bot.SendMessage(chatId, "Неверный формат. Используйте: `ДД.ММ - ДД.ММ`", 
                                   parseMode: ParseMode.Markdown, cancellationToken: ct);
            return;
        }

        var dayStart = int.Parse(match.Groups[1].Value);
        var monthStart = int.Parse(match.Groups[2].Value);
        var dayEnd = int.Parse(match.Groups[3].Value);
        var monthEnd = int.Parse(match.Groups[4].Value);

        var now = DateTime.UtcNow;
        var checkIn = new DateTime(now.Year, monthStart, dayStart);
        var checkOut = new DateTime(now.Year, monthEnd, dayEnd);

        if (checkOut <= checkIn)
        {
            await _bot.SendMessage(chatId, "Дата выезда должна быть позже даты заезда!", cancellationToken: ct);
            return;
        }

        state.CheckIn = checkIn;
        state.CheckOut = checkOut;
        state.CurrentStep = BookingStep.ConfirmingBooking;

        var confirmText = $"Подтвердите бронирование:\n\n" +
                          $"Квартира ID: {state.SelectedApartmentId}\n" +
                          $"Заезд: {checkIn:dd.MM.yyyy}\n" +
                          $"Выезд: {checkOut:dd.MM.yyyy}\n" +
                          $"Ночей: {(checkOut - checkIn).Days}\n\n" +
                          $"Напишите `подтверждаю` для создания брони или `отмена` для возврата.";

        await _bot.SendMessage(chatId, confirmText, cancellationToken: ct);
    }

    private async Task HandleConfirmationInputAsync(long chatId, string text, BotState state, CancellationToken ct)
    {
        if (text.ToLower().Contains("подтвержд"))
        {
            var userResult = await _userService.GetOrCreateUserAsync(chatId, "temp", ct);
            if (!userResult.IsSuccess || userResult.User?.Id is not Guid tenantId)
            {
                await _bot.SendMessage(chatId, "Ошибка пользователя. Попробуйте /start", cancellationToken: ct);
                return;
            }

            try
            {
                var period = new DateRange(state.CheckIn!.Value, state.CheckOut!.Value);
                var booking = await _bookingService.CreateAsync(
                    tenantId: tenantId,
                    apartmentId: state.SelectedApartmentId!.Value,
                    period: period,
                    ct: ct);

                await _bot.SendMessage(
                    chatId, 
                    $"Бронь создана!\n" +
                    $"ID: {booking.Id.ToString()[..8]}...\n" +
                    $"{period.Start:dd.MM} — {period.End:dd.MM}\n" +
                    $"Итоговая стоимость: {booking.TotalPrice.Amount} ₽\n" +
                    $"Статус: Ожидает админа", 
                    cancellationToken: ct);
            }
            catch (Exception ex)
            {
                await _bot.SendMessage(chatId, $"Ошибка бронирования: {ex.Message}", cancellationToken: ct);
            }
        }
        else if (text.ToLower().Contains("отмен"))
        {
            await _bot.SendMessage(chatId, "Бронирование отменено.", cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId, "Напишите `подтверждаю` или `отмена`", cancellationToken: ct);
            return;
        }

        _userStates.TryRemove(chatId, out _);
    }

    private async Task HandleStateInput(long chatId, string text, CancellationToken ct)
    {
        if (!_userStates.TryGetValue(chatId, out var state)) return;

        switch (state.CurrentStep)
        {
            case BookingStep.SelectingDates:
                await HandleDateInputAsync(chatId, text, state, ct);
                break;
            case BookingStep.ConfirmingBooking:
                await HandleConfirmationInputAsync(chatId, text, state, ct);
                break;
            default:
                _userStates.TryRemove(chatId, out _);
                break;
        }
    }

    private bool IsAdmin(long chatId) => _adminChatIds.Contains(chatId);

    private async Task HandlePendingBookings(long chatId, CancellationToken ct)
    {
        if (!IsAdmin(chatId))
        {
            await _bot.SendMessage(chatId, "Доступ запрещён. Только для администраторов.", cancellationToken: ct);
            return;
        }

        var pending = await _bookingService.GetPendingBookingsAsync(ct);
        var list = pending.ToList();

        if (!list.Any())
        {
            await _bot.SendMessage(chatId, "Нет ожидающих подтверждения броней.", cancellationToken: ct);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("<b>Заявки на бронирование:</b>\n");

        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var b in list)
        {
            var userName = _tenantToChatId.TryGetValue(b.TenantId, out var uChatId) ? $"(User {uChatId})" : "";
            
            sb.AppendLine($"#{b.Id.ToString()[..8]} {userName}");
            sb.AppendLine($"{b.Period.Start:dd.MM} — {b.Period.End:dd.MM} ({(b.Period.End - b.Period.Start).Days} ночей)");
            sb.AppendLine($"{b.TotalPrice.Amount:N0} ₽");

            buttons.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("Одобрить", $"admin_approve_{b.Id}"),
                InlineKeyboardButton.WithCallbackData("Отклонить", $"admin_reject_{b.Id}")
            });
        }

        await _bot.SendMessage(
            chatId, 
            sb.ToString(), 
            replyMarkup: new InlineKeyboardMarkup(buttons),
            parseMode: ParseMode.Html,
            cancellationToken: ct);
    }

    private async Task HandleAdminApprove(long chatId, Guid bookingId, CancellationToken ct)
    {
        if (!IsAdmin(chatId)) return;

        try
        {
            var booking = await _bookingService.ApproveBookingAsync(bookingId, ct);
            await NotifyTenantAsync(booking.TenantId, 
                $"Ваша бронь #{booking.Id.ToString()[..8]} одобрена администратором!\n" +
                $"{booking.Period.Start:dd.MM} — {booking.Period.End:dd.MM}", ct);
            await _bot.SendMessage(chatId, $"Бронь #{booking.Id.ToString()[..8]} одобрена.", cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await _bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
        }
    }

    private async Task HandleAdminReject(long chatId, Guid bookingId, CancellationToken ct)
    {
        if (!IsAdmin(chatId)) return;

        try
        {
            var booking = await _bookingService.RejectBookingAsync(bookingId, "Отклонено администратором", ct);
            await NotifyTenantAsync(booking.TenantId, 
                $"Ваша бронь #{booking.Id.ToString()[..8]} отклонена.", ct);
            await _bot.SendMessage(chatId, $"Бронь #{booking.Id.ToString()[..8]} отклонена.", cancellationToken: ct);
        }
        catch (Exception ex)
        {
            await _bot.SendMessage(chatId, $"Ошибка: {ex.Message}", cancellationToken: ct);
        }
    }

    private async Task NotifyTenantAsync(Guid tenantId, string message, CancellationToken ct)
    {
        if (_tenantToChatId.TryGetValue(tenantId, out var chatId))
        {
            try
            {
                await _bot.SendMessage(chatId, message, cancellationToken: ct);
            }
            catch
            {
                System.Console.WriteLine($"Не удалось отправить уведомление пользователю {tenantId}");
            }
        }
        else
        {
            System.Console.WriteLine($"ChatId не найден для TenantId: {tenantId}");
        }
    }

    private async Task HandleStartBooking(long chatId, CancellationToken ct)
    {
        await _bot.SendMessage(chatId, "Используйте команду /apartments для начала бронирования.", cancellationToken: ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken ct)
    {
        var errorMessage = exception switch
        {
            ApiRequestException api => $"[Telegram Error]: {api.Message}",
            _ => exception.ToString()
        };
        System.Console.WriteLine($"{errorMessage}");
        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(long chatId, string text, CancellationToken ct = default)
    {
        try
        {
            await _bot.SendMessage(chatId, text, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Ошибка отправки в чат {chatId}: {ex.Message}");
        }
    }
}