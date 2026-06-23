using System.Text;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Db.Repositories;

public class PostgresBookingRepository : IBookingRepository
{
    /// <summary>
    /// DTO для маппинга из БД
    /// </summary>
    private class BookingDto
    {
        public Guid id { get; set; }
        public Guid tenant_id { get; set; }
        public Guid apartment_id { get; set; }
        public DateTime check_in_date { get; set; }
        public DateTime check_out_date { get; set; }
        public string status { get; set; } = null!;
        public decimal total_price { get; set; }
        public decimal deposit { get; set; }
        public string? cancellation_reason { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
    
    /// <summary>
    /// Объект с подключением, управлением транзакциями и списком событий
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;
    
    public PostgresBookingRepository(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    /// <summary>
    /// Функция для маппинга из DTO -> Domain Entity
    /// </summary>
    private static Booking MapToBooking(BookingDto dto)
    {
        return new Booking(
            id: dto.id,
            tenantId: dto.tenant_id,
            apartmentId: dto.apartment_id,
            period: new DateRange(dto.check_in_date, dto.check_out_date),
            totalPrice: new Money(dto.total_price),
            deposit: new Money(dto.deposit),
            status: Enum.Parse<BookingStatus>(dto.status, ignoreCase: true),
            cancellationReason: dto.cancellation_reason,
            createdAt: dto.created_at,
            updatedAt: dto.updated_at
        );
    }
    
    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, apartment_id,
                check_in_date, check_out_date,
                status, total_price, deposit,
                cancellation_reason, created_at, updated_at
            FROM bookings 
            WHERE id = @Id";
        
        var dto = await _unitOfWork.Connection!
            .QueryFirstOrDefaultAsync<BookingDto>(
                sql, 
                new { Id = id }, 
                transaction: _unitOfWork.Transaction);
        
        return dto == null ? null : MapToBooking(dto);
    }
    
    public async Task<IEnumerable<Booking>> GetByTenantIdAsync(Guid tenantId)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, apartment_id,
                check_in_date, check_out_date,
                status, total_price, deposit,
                cancellation_reason, created_at, updated_at
            FROM bookings 
            WHERE tenant_id = @TenantId 
            ORDER BY created_at DESC";

        var dtos = await _unitOfWork.Connection!
            .QueryAsync<BookingDto>(sql, new { TenantId = tenantId });

        return dtos.Select(MapToBooking);
    }
    
    public async Task<IEnumerable<Booking>> GetByApartmentIdAsync(Guid apartmentId)
    {
        const string sql = @"
            SELECT 
                id, tenant_id, apartment_id,
                check_in_date, check_out_date,
                status, total_price, deposit,
                cancellation_reason, created_at, updated_at
            FROM bookings 
            WHERE apartment_id = @ApartmentId 
            ORDER BY created_at DESC";

        var dtos = await _unitOfWork.Connection!
            .QueryAsync<BookingDto>(sql, new { ApartmentId = apartmentId });

        return dtos.Select(MapToBooking);
    }
    
    public async Task<IEnumerable<Booking>> GetPendingAsync()
    {
        const string sql = @"
            SELECT 
                id, tenant_id, apartment_id,
                check_in_date, check_out_date,
                status, total_price, deposit,
                cancellation_reason, created_at, updated_at
            FROM bookings 
            WHERE status = 'Pending' 
            ORDER BY created_at DESC";
        
        var dtos = await _unitOfWork.Connection!
            .QueryAsync<BookingDto>(sql);
        
        return dtos.Select(MapToBooking);
    }
    
    public async Task<IEnumerable<Booking>> GetFilteredAsync(BookingFilterCriteria criteria)
    {
        var sql = new StringBuilder(@"
        SELECT 
            id, tenant_id, apartment_id,
            check_in_date, check_out_date,
            status, total_price, deposit,
            cancellation_reason, 
            created_at, updated_at
        FROM bookings 
        WHERE 1=1");

        var parameters = new DynamicParameters();

        if (criteria.TenantId.HasValue)
        {
            sql.Append(" AND tenant_id = @TenantId");
            parameters.Add("TenantId", criteria.TenantId);
        }
        if (!string.IsNullOrEmpty(criteria.Status))
        {
            sql.Append(" AND status = @Status");
            parameters.Add("Status", criteria.Status);
        }
        if (criteria.FromDate.HasValue)
        {
            sql.Append(" AND check_in_date >= @FromDate");
            parameters.Add("FromDate", criteria.FromDate);
        }
        if (criteria.ToDate.HasValue)
        {
            sql.Append(" AND check_in_date <= @ToDate");
            parameters.Add("ToDate", criteria.ToDate);
        }
        sql.Append(" ORDER BY created_at DESC");
        
        var dtos = await _unitOfWork.Connection!
            .QueryAsync<BookingDto>(sql.ToString(), parameters);

        return dtos.Select(MapToBooking);
    }

    public async Task AddAsync(Booking booking)
    {
        const string sql = @"
            INSERT INTO bookings (
                id, tenant_id, apartment_id, 
                check_in_date, check_out_date, 
                status, total_price, deposit, 
                created_at
            ) VALUES (
                @Id, @TenantId, @ApartmentId,
                @CheckInDate, @CheckOutDate,
                @Status, @TotalPrice, @Deposit,
                @CreatedAt
            )";
        
        await _unitOfWork.Connection!.ExecuteAsync(sql, new
        {
            booking.Id,
            booking.TenantId,
            booking.ApartmentId,
            CheckInDate = booking.Period.Start.Date,
            CheckOutDate = booking.Period.End.Date,
            Status = booking.Status.ToString(),
            TotalPrice = booking.TotalPrice.Amount,
            Deposit = booking.Deposit.Amount,
            booking.CreatedAt
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task UpdateAsync(Booking booking)
    {
        const string sql = @"
            UPDATE bookings SET 
                tenant_id = @TenantId,
                apartment_id = @ApartmentId,
                check_in_date = @CheckInDate,
                check_out_date = @CheckOutDate,
                status = @Status,
                total_price = @TotalPrice,
                deposit = @Deposit,
                cancellation_reason = @CancellationReason,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await _unitOfWork.Connection!.ExecuteAsync(sql, new
        {
            booking.Id,
            booking.TenantId,
            booking.ApartmentId,
            CheckInDate = booking.Period.Start.Date,
            CheckOutDate = booking.Period.End.Date,
            Status = booking.Status.ToString(),
            TotalPrice = booking.TotalPrice.Amount,
            Deposit = booking.Deposit.Amount,
            booking.CancellationReason,
            booking.UpdatedAt
        }, transaction: _unitOfWork.Transaction);
        
        // Берем все события, которые накопились в сущности, и кладем их в очередь ожидания (_pendingEvents)
        if (booking.DomainEvents.Any())
        {
            // DomainEvents передает копию списка, чтобы UnitOfWork мог их очистить после коммита
            _unitOfWork.RegisterDomainEvents(booking.DomainEvents);
            booking.ClearDomainEvents();
        }
    }
    
    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM bookings WHERE id = @Id";
        await _unitOfWork.Connection!.ExecuteAsync(
            sql, 
            new { Id = id }, 
            transaction: _unitOfWork.Transaction);
    }
}