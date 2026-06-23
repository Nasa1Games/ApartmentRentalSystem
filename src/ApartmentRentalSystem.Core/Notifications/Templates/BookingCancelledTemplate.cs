namespace ApartmentRentalSystem.Core.Notifications.Templates;

public class BookingCancelledTemplate : NotificationTemplate
{
    public override string Subject => "⚠️ Бронь отменена";
    
    public override string Body => """
                                   Здравствуйте, {TenantName}.

                                   Ваша бронь квартиры №{ApartmentNumber} была отменена.

                                   Причина: {Reason}

                                   Если это ошибка или у вас есть вопросы, пожалуйста, свяжитесь с администрацией.
                                   """;
}