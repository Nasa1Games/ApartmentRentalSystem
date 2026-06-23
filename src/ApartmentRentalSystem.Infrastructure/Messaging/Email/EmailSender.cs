namespace ApartmentRentalSystem.Infrastructure.Messaging;

public class EmailSender : IEmailSender
{
    public async Task SendAsync(string email, string subject, string body, CancellationToken ct)
    {
        // Тут логика отправки через SMTP (MailKit и т.д.)
        Console.WriteLine($"[Email] To: {email} | Subject: {subject}");
        await Task.CompletedTask;
    }
}