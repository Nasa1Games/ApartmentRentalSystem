using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Application.Services;

namespace ApartmentRentalSystem.Infrastructure.Telegram;

public class TelegramBotService : ITelegramBotService
{
    private readonly TelegramBotClient _bot;
    private readonly IConfiguration _config;
    private readonly ITelegramUserService _userService; // ← Зависимость от Application!
    private CancellationTokenSource? _cts;
    private bool _isRunning;

    public TelegramBotService(IConfiguration config, ITelegramUserService userService)
    {
        _config = config;
        _userService = userService;
        
        var token = _config["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Не найден Telegram:BotToken в конфигурации");
            
        _bot = new TelegramBotClient(token);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        if (_isRunning) return;
        
        var me = await _bot.GetMe(ct);
        System.Console.WriteLine($"✅ Бот подключен: @{me.Username}");

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
        
        System.Console.WriteLine("🛑 Остановка бота...");
        
        if (_cts != null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        _isRunning = false;
    }

    private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message || update.Message?.Text is not string text)
            return;

        var chatId = update.Message.Chat.Id;
        var username = update.Message.From?.Username ?? "User";

        System.Console.WriteLine($"💬 Сообщение от {username} ({chatId}): {text}");

        switch (text.ToLower())
        {
            case "/start":
                await HandleStartCommand(chatId, username, ct);
                break;
            case "/help":
                await client.SendMessage(chatId, "Команды: /start, /help", cancellationToken: ct);
                break;
            default:
                await client.SendMessage(chatId, "Неизвестная команда. Введите /help", cancellationToken: ct);
                break;
        }
    }

    private async Task HandleStartCommand(long chatId, string username, CancellationToken ct)
    {
        // 🔹 Вызываем бизнес-логику из Application слоя
        var result = await _userService.GetOrCreateUserAsync(chatId, username, ct);
        
        if (result.IsSuccess)
        {
            var message = result.IsNewRegistration
                ? $"✅ Регистрация успешна! Привет, {username}!"
                : $"👋 С возвращением, {result.User?.FullName}!";
            
            await _bot.SendMessage(chatId, message, cancellationToken: ct);
        }
        else
        {
            await _bot.SendMessage(chatId, $"❌ Ошибка: {result.ErrorMessage}", cancellationToken: ct);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken ct)
    {
        var errorMessage = exception switch
        {
            ApiRequestException api => $"[Telegram Error]: {api.Message}",
            _ => exception.ToString()
        };
        System.Console.WriteLine($"❌ {errorMessage}");
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
            System.Console.WriteLine($"❌ Ошибка отправки в чат {chatId}: {ex.Message}");
        }
    }
}