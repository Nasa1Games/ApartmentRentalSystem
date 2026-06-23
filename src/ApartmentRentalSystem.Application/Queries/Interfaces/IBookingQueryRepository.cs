using ApartmentRentalSystem.Application.Dto;

namespace ApartmentRentalSystem.Application.Interfaces;

public interface IBookingQueryRepository
{
    // - - Для пользователя - -
    /// <summary>
    /// Получить все брони конкретного пользователя (для Telegram бота)
    /// </summary>
    Task<IEnumerable<UserBookingDto>> GetBookingsByTenantIdAsync(Guid tenantId);
    
    
    // - - Для админа - -
    /// <summary>
    /// Получить все заявки в статусе Pending (для админки)
    /// </summary>
    Task<IEnumerable<PendingBookingDto>> GetPendingBookingsAsync();
    
    /// <summary>
    /// Получить полную информацию по одной заявке (для детального просмотра)
    /// </summary>
    Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId);
    
    /// <summary>
    /// Получить список заявок с применением фильтров (подъезд, этаж, даты, цена, вместимость, статус)
    /// </summary>
    Task<IEnumerable<FilteredBookingDto>> GetFilteredBookingsAsync(
        int? entrance = null,
        int? floor = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minCapacity = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null);
}