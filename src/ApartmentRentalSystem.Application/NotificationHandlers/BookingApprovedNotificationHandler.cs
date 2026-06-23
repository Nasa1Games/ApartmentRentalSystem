using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Events;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.Notifications.Templates;
using MediatR;

namespace ApartmentRentalSystem.Application.Handlers;

public class BookingApprovedNotificationHandler : INotificationHandler<BookingApprovedEvent>
{
    // Событие содержит только ID (чтобы быть легким),
    // поэтому нам нужно подтянуть полные объекты -- подключаем репозитории
    private readonly ITenantRepository _tenantRepository;
    private readonly IApartmentRepository _apartmentRepository;
    private readonly INotificationSender _notificationSender;
    
    public BookingApprovedNotificationHandler(
        ITenantRepository tenantRepository,
        IApartmentRepository apartmentRepository,
        INotificationSender notificationSender)
    {
        _tenantRepository = tenantRepository;
        _apartmentRepository = apartmentRepository;
        _notificationSender = notificationSender;
    }

    
    // Метод, который MediatR вызовет автоматически после публикации события
    public async Task Handle(BookingApprovedEvent @event, CancellationToken cancellationToken)
    {
        // - - Загружаем данные из БД - -
        var tenant = await _tenantRepository.GetByIdAsync(@event.TenantId);
        var apartment = await _apartmentRepository.GetByIdAsync(@event.ApartmentId);

        // Если данные удалены или повреждены — просто выходим, чтобы не упасть с ошибкой
        if (tenant == null || apartment == null)
            return;

        
        // - - Рендерим текст уведомления - -
        var template = new BookingApprovedTemplate();
        
        // Подставляем данные в поля шаблона: {TenantName}, {ApartmentNumber} и т.д.
        var subject = template.Subject;
        var body = template.Render(
            ("TenantName", tenant.Contact.FullName),
            ("ApartmentNumber", apartment.UnitNumber),
            ("StartDate", @event.Period.Start.ToShortDateString()),
            ("EndDate", @event.Period.End.ToShortDateString()),
            ("TotalPrice", @event.TotalPrice.Amount.ToString("F0")),
            ("Address", $"{apartment.Entrance} подъезд, {apartment.Floor} этаж")
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
                
                // Ничего не привязано
                //default:
            }
        }
        catch
        {
            // Пустой catch для фонового обработчиков событий.
            // Админ подтвердил бронь.
            // Данные в базе сохранились (COMMIT прошел).
            // Запускается этот обработчик, но у сервера временно пропал интернет,
            // и Telegram API вернул ошибку.
            // Если мы пробросим исключение (throw),
            // оно может "откатить" транзакцию или вызвать крах UI-потока админки.
            // Поскольку данные в базе уже корректны, мы просто "глотаем" ошибку отправки.
            // Уведомление не дойдет, но система останется в целостном состоянии.
        }
    }
}