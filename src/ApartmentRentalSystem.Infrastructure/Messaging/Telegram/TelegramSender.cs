using Telegram.Bot;

namespace ApartmentRentalSystem.Infrastructure.Messaging;

public class TelegramSender : ITelegramSender
{
    private readonly ITelegramBotClient _bot;
    
    public TelegramSender(ITelegramBotClient bot) => _bot = bot;

    public async Task SendAsync(long chatId, string message, CancellationToken ct)
    {
        await _bot.SendMessage(chatId, message, cancellationToken: ct);
    }
}