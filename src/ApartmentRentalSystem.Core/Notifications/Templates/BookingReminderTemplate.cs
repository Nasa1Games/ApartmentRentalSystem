namespace ApartmentRentalSystem.Core.Notifications.Templates;

public class BookingReminderTemplate : NotificationTemplate
{
    public override string Subject => "⏰ Напоминание о брони";
    
    public override string Body => """
                                   Напоминаем о вашей брони!

                                   🏠 Квартира №{ApartmentNumber}
                                   📅 Заезд: {StartDate}
                                   ⏱️ Осталось: {HoursUntilCheckIn} ч.

                                   Ждём вас!
                                   """;
}