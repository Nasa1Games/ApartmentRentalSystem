using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Events;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.Notifications.Templates;
using MediatR;

namespace ApartmentRentalSystem.Application.Handlers;

public class BookingCancelledNotificationHandler : INotificationHandler<BookingCancelledEvent>
{
    // Событие содержит только ID (чтобы быть легким),
    // поэтому нам нужно подтянуть полные объекты -- подключаем репозитории
    private readonly ITenantRepository _tenantRepository;
    private readonly IApartmentRepository _apartmentRepository;
    private readonly INotificationSender _notificationSender;

    public BookingCancelledNotificationHandler(
        ITenantRepository tenantRepository,
        IApartmentRepository apartmentRepository,
        INotificationSender notificationSender)
    {
        _tenantRepository = tenantRepository;
        _apartmentRepository = apartmentRepository;
        _notificationSender = notificationSender;
    }

    
    // Метод, который MediatR вызовет автоматически после публикации события
    public async Task Handle(BookingCancelledEvent @event, CancellationToken cancellationToken)
    {
        // - - Загружаем данные из БД - -
        var tenant = await _tenantRepository.GetByIdAsync(@event.TenantId);
        var apartment = await _apartmentRepository.GetByIdAsync(@event.ApartmentId);

        if (tenant == null || apartment == null)
            return;

        // - - Рендерим текст уведомления - -
        var template = new BookingCancelledTemplate();

        var subject = template.Subject;
        var body = template.Render(
            ("TenantName", tenant.Contact.FullName),
            ("ApartmentNumber", apartment.UnitNumber),
            ("Reason", string.IsNullOrWhiteSpace(@event.Reason) 
                ? "Не указана" 
                : @event.Reason) // Защита от пустой причины
        );

        
        // - - Отправка (Приложение само выбирает канал) - -
        try
        {
            switch (tenant.PreferredChannel)
            {
                // Клиент хочет Telegram и у него он прикреплен
                case NotificationChannel.Telegram when tenant.TelegramId > 0:
                {
                    await _notificationSender.SendViaTelegramAsync(
                        tenant.TelegramId, subject, body, cancellationToken);
                    break;
                }
                
                // Клиент хочет Telegram и у него нет Telegram ID, но есть почта
                case NotificationChannel.Telegram:
                {
                    if (!string.IsNullOrWhiteSpace(tenant.Email))
                    {
                        await _notificationSender.SendViaEmailAsync(
                            tenant.Email, subject, body, cancellationToken);
                    }
                    break;
                }
                
                // Клиент хочет Email и у него есть Email
                case NotificationChannel.Email when !string.IsNullOrWhiteSpace(tenant.Email):
                {
                    await _notificationSender.SendViaEmailAsync(
                        tenant.Email, subject, body, cancellationToken);
                    break;
                }
                
                // Клиент хочет почты и ее нет, но есть Telegram ID
                case NotificationChannel.Email:
                {
                    if (tenant.TelegramId > 0)
                    {
                        await _notificationSender.SendViaTelegramAsync(
                            tenant.TelegramId, subject, body, cancellationToken);
                    }
                    break;
                }
                
                // Ничего не нашлось
                // default:
            }
        }
        catch
        {
            // Ошибки отправки игнорируем, чтобы не ломать основной поток
        }
    }
}