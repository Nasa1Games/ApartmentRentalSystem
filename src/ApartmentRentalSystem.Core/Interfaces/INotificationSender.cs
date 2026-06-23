namespace ApartmentRentalSystem.Core.Interfaces;

public interface INotificationSender
{
    // Просто отправляем,
    // логика отправки -- application,
    // реализация отправки -- infrastructure 
    
    Task SendViaTelegramAsync(long chatId, string subject, string body, CancellationToken ct);
    Task SendViaEmailAsync(string email, string subject, string body, CancellationToken ct);
}