namespace ApartmentRentalSystem.Core.Interfaces;

public interface ITelegramBotService
{
    Task StartAsync(CancellationToken ct);  // Запуск прослушивания сообщений (фон)
    
    Task StopAsync(CancellationToken ct);  // Остановка
    
    Task SendMessageAsync(long chatId, string text, CancellationToken ct = default);
}