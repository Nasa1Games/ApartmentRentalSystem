namespace ApartmentRentalSystem.Core.Notifications.Templates;

public class EmergencyNotificationTemplate : NotificationTemplate
{
    public override string Subject => "🚨 Важное уведомление";
    
    public override string Body => """
                                   Внимание!

                                   {Message}

                                   Просим отнестись с пониманием.
                                   """;
}