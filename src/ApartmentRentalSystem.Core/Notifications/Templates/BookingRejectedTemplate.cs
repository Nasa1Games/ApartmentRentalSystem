namespace ApartmentRentalSystem.Core.Notifications.Templates;

public class BookingRejectedTemplate : NotificationTemplate
{
    public override string Subject => "❌ Бронь отклонена";
    
    public override string Body => """
                                   Здравствуйте, {TenantName}.

                                   К сожалению, ваша бронь квартиры №{ApartmentNumber} была отклонена.

                                   Причина: {Reason}

                                   Вы можете создать новую бронь с другими датами.
                                   """;
}