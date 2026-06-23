namespace ApartmentRentalSystem.Infrastructure.Messaging;

public interface IEmailSender
{
    Task SendAsync(string email, string subject, string body, CancellationToken ct);
}