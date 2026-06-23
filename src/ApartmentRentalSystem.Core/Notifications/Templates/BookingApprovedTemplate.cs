namespace ApartmentRentalSystem.Core.Notifications.Templates;

public class BookingApprovedTemplate : NotificationTemplate
{
    public override string Subject => "✅ Бронь подтверждена";
    
    public override string Body => """
                                   Здравствуйте, {TenantName}!
                                   Ваша бронь квартиры №{ApartmentNumber} подтверждена.

                                   📅 Период: {StartDate} — {EndDate}
                                   💰 Общая стоимость: {TotalPrice} ₽
                                   🏠 Адрес: {Address}

                                   До встречи!
                                   """;
}