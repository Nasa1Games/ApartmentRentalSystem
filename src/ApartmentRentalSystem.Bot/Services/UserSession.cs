using ApartmentRentalSystem.Core.Enums;

namespace ApartmentRentalSystem.Bot.Services;

public class UserSession
{
    public long ChatId { get; set; }
    public Guid? TenantId { get; set; }
    public UserState State { get; set; } = UserState.None;
    
    // Фильтры для поиска квартир
    public SearchFilters? SearchFilters { get; set; }
    
    // Данные для создания брони
    public Guid? SelectedApartmentId { get; set; }
    public DateTime? CheckInDate { get; set; }
    public DateTime? CheckOutDate { get; set; }
    public int PartySize { get; set; } = 1;
    
    public UserSession(long chatId)
    {
        ChatId = chatId;
    }
    
    public void ResetBookingData()
    {
        SelectedApartmentId = null;
        CheckInDate = null;
        CheckOutDate = null;
        PartySize = 1;
    }
}

public enum UserState
{
    None,
    SearchingApartments,
    BookingSelectApartment,
    BookingEnterCheckIn,
    BookingEnterCheckOut,
    BookingConfirm
}

public class SearchFilters
{
    public int? MinFloor { get; set; }
    public int? MaxFloor { get; set; }
    public int? MinCapacity { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public ApartmentStatus? Status { get; set; } = ApartmentStatus.Available;
    
    public void Reset()
    {
        MinFloor = null;
        MaxFloor = null;
        MinCapacity = null;
        MinPrice = null;
        MaxPrice = null;
        Status = ApartmentStatus.Available;
    }
    
    public bool HasAnyFilter()
    {
        return MinFloor.HasValue || MaxFloor.HasValue || 
               MinCapacity.HasValue || MinPrice.HasValue || MaxPrice.HasValue;
    }
}