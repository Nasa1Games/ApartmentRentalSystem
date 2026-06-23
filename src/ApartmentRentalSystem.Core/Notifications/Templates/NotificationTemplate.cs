namespace ApartmentRentalSystem.Core.Notifications.Templates;

public abstract class NotificationTemplate
{
    public abstract string Subject { get; }
    public abstract string Body { get; }

    public string Render(params (string Key, string Value)[] replacements)
    {
        var result = Body;
        foreach (var (key, value) in replacements)
        {
            // Заменяем {Key} на value
            result = result.Replace($"{{{{{key}}}}}", value ?? string.Empty);
        }
        return result;
    }
}