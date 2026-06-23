using System.Text;
using ApartmentRentalSystem.Application.Dto;
using ApartmentRentalSystem.Application.Interfaces;
using ApartmentRentalSystem.Infrastructure.Db.Connection;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Db.Repositories;

public class PostgresBookingQueryRepository : IBookingQueryRepository
{
    private readonly IDbConnectionFactory _factory;

    public PostgresBookingQueryRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<UserBookingDto>> GetBookingsByTenantIdAsync(Guid tenantId)
    {
        await using var conn = _factory.CreateConnection();
        
        const string sql = @"
            SELECT 
                b.id AS BookingId,
                a.unit_number AS ApartmentUnitNumber,
                a.entrance AS ApartmentEntrance,
                a.floor AS ApartmentFloor,
                b.check_in_date AS CheckInDate,
                b.check_out_date AS CheckOutDate,
                b.status AS Status,
                b.total_price AS TotalPrice
            FROM bookings b
            JOIN apartments a ON b.apartment_id = a.id
            WHERE b.tenant_id = @TenantId
            ORDER BY b.check_in_date DESC";

        return await conn.QueryAsync<UserBookingDto>(sql, new { TenantId = tenantId });
    }
    
    public async Task<IEnumerable<PendingBookingDto>> GetPendingBookingsAsync()
    {
        await using var conn = _factory.CreateConnection();
        
        const string sql = @"
            SELECT 
                b.id AS BookingId,
                t.full_name AS TenantFullName,
                a.unit_number AS ApartmentUnitNumber,
                b.check_in_date AS CheckInDate,
                b.check_out_date AS CheckOutDate,
                b.total_price AS TotalPrice,
                b.status AS Status
            FROM bookings b
            JOIN tenants t ON b.tenant_id = t.id
            JOIN apartments a ON b.apartment_id = a.id
            WHERE b.status = 'Pending'
            ORDER BY b.created_at DESC";

        return await conn.QueryAsync<PendingBookingDto>(sql);
    }
    
    public async Task<BookingDetailsDto?> GetBookingDetailsAsync(Guid bookingId)
    {
        await using var conn = _factory.CreateConnection();
        
        const string sql = @"
            SELECT 
                b.id AS BookingId,
                b.tenant_id AS TenantId,
                b.check_in_date AS CheckInDate,
                b.check_out_date AS CheckOutDate,
                b.total_price AS TotalPrice,
                b.deposit AS Deposit,
                b.status AS Status,
                b.cancellation_reason AS CancellationReason,
                b.created_at AS CreatedAt,
                b.updated_at AS UpdatedAt,
                
                t.full_name AS TenantFullName,
                t.email AS TenantEmail,
                t.phone AS TenantPhone,
                t.telegram_id AS TenantTelegramId,
                
                a.unit_number AS ApartmentUnitNumber,
                a.entrance AS ApartmentEntrance,
                a.floor AS ApartmentFloor,
                a.capacity AS ApartmentCapacity,
                a.base_price_per_night AS ApartmentPricePerNight
            FROM bookings b
            JOIN tenants t ON b.tenant_id = t.id
            JOIN apartments a ON b.apartment_id = a.id
            WHERE b.id = @BookingId";

        // QueryFirstOrDefaultAsync вернет null, если заявка не найдена
        return await conn.QueryFirstOrDefaultAsync<BookingDetailsDto>(sql, new { BookingId = bookingId });
    }

    public async Task<IEnumerable<FilteredBookingDto>> GetFilteredBookingsAsync(
        int? entrance = null,
        int? floor = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? minCapacity = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? status = null)
    {
        await using var conn = _factory.CreateConnection();
        
        var sql = new StringBuilder(@"
            SELECT 
                b.id AS BookingId,
                t.full_name AS TenantFullName,
                a.unit_number AS ApartmentUnitNumber,
                a.entrance AS ApartmentEntrance,
                a.floor AS ApartmentFloor,
                a.capacity AS ApartmentCapacity,
                b.total_price AS TotalPrice,
                b.check_in_date AS CheckInDate,
                b.check_out_date AS CheckOutDate,
                b.status AS Status
            FROM bookings b
            JOIN tenants t ON b.tenant_id = t.id
            JOIN apartments a ON b.apartment_id = a.id
            WHERE 1=1"); // 1=1 нужно, чтобы удобно добавлять условия через AND

        var parameters = new DynamicParameters();
        
        // Динамическое построение WHERE
        if (entrance.HasValue)
        {
            sql.Append(" AND a.entrance = @Entrance");
            parameters.Add("Entrance", entrance);
        }
        if (floor.HasValue)
        {
            sql.Append(" AND a.floor = @Floor");
            parameters.Add("Floor", floor);
        }
        if (fromDate.HasValue)
        {
            sql.Append(" AND b.check_in_date >= @FromDate");
            parameters.Add("FromDate", fromDate.Value.Date);
        }
        if (toDate.HasValue)
        {
            sql.Append(" AND b.check_in_date <= @ToDate");
            parameters.Add("ToDate", toDate.Value.Date);
        }
        if (minCapacity.HasValue)
        {
            sql.Append(" AND a.capacity >= @MinCapacity");
            parameters.Add("MinCapacity", minCapacity);
        }
        if (minPrice.HasValue)
        {
            sql.Append(" AND b.total_price >= @MinPrice");
            parameters.Add("MinPrice", minPrice);
        }
        if (maxPrice.HasValue)
        {
            sql.Append(" AND b.total_price <= @MaxPrice");
            parameters.Add("MaxPrice", maxPrice);
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            sql.Append(" AND b.status = @Status");
            parameters.Add("Status", status);
        }

        sql.Append(" ORDER BY b.created_at DESC");

        return await conn.QueryAsync<FilteredBookingDto>(sql.ToString(), parameters);
    }
}