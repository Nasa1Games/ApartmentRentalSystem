namespace ApartmentRentalSystem.Infrastructure.Telegram;

public class BotState
{
    public Guid? SelectedApartmentId { get; set; }
    public DateTime? CheckIn { get; set; }
    public DateTime? CheckOut { get; set; }
    public BookingStep CurrentStep { get; set; }
}