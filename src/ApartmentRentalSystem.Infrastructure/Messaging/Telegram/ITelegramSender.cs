namespace ApartmentRentalSystem.Infrastructure.Messaging;

public interface ITelegramSender
{
    Task SendAsync(long chatId, string message, CancellationToken ct);
}