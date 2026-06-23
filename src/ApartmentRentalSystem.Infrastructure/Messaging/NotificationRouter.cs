using ApartmentRentalSystem.Core.Interfaces;

namespace ApartmentRentalSystem.Infrastructure.Messaging;

public class NotificationRouter : INotificationSender
{
    private readonly ITelegramSender _telegramSender;
    private readonly IEmailSender _emailSender;

    public NotificationRouter(ITelegramSender telegramSender, IEmailSender emailSender)
    {
        _telegramSender = telegramSender;
        _emailSender = emailSender;
    }
    
    public async Task SendViaTelegramAsync(long chatId, string subject, string body, CancellationToken ct)
    {
        // для тг свой сервис в целом не нужен, но оставим для единого синтаксиса методов
        await _telegramSender.SendAsync(chatId, $"{subject}\n{body}", ct);
    }
    
    public async Task SendViaEmailAsync(string email, string subject, string body, CancellationToken ct)
    {
        await _emailSender.SendAsync(email, subject, body, ct);
    }
    
}