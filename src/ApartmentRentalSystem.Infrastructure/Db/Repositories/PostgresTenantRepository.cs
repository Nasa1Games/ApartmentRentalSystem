using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using Dapper;


namespace ApartmentRentalSystem.Infrastructure.Db.Repositories;

public class PostgresTenantRepository : ITenantRepository
{
    /// <summary>
    /// DTO для маппинга из БД
    /// </summary>
    private class TenantDto
    {
        public Guid id { get; set; }
        public string email { get; set; } = null!;
        public string full_name { get; set; } = null!;
        public string? phone { get; set; }
        public long telegram_id { get; set; }
        public int preferred_channel { get; set; }
        public bool is_blocked { get; set; }
        public DateTime created_at { get; set; }
    }
    
    /// <summary>
    /// Объект с подключением, управлением транзакциями и списком событий
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;
    
    public PostgresTenantRepository(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    /// <summary>
    /// Функция для маппинга из DTO -> Domain Entity
    /// </summary>
    private static Tenant MapToTenant(TenantDto row)
    {
        return new Tenant(
            id: row.id,
            email: row.email,
            contact: new ContactInfo(row.full_name, row.phone),
            telegramId: row.telegram_id,
            preferredChannel: (NotificationChannel)row.preferred_channel,
            isBlocked: row.is_blocked,
            createdAt: row.created_at
        );
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT 
                id, email, full_name, phone, 
                telegram_id, preferred_channel, is_blocked, created_at
            FROM tenants 
            WHERE id = @Id";

        var dto = await _unitOfWork.Connection!
            .QueryFirstOrDefaultAsync<TenantDto>(sql, new { Id = id });
        
        return dto == null ? null : MapToTenant(dto);
    }

    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT 
                id, email, full_name, phone, 
                telegram_id, preferred_channel, is_blocked, created_at
            FROM tenants 
            WHERE email = @Email";
        
        var dto = await _unitOfWork.Connection!
            .QueryFirstOrDefaultAsync<TenantDto>(sql, new { Email = email });

        return dto == null ? null : MapToTenant(dto);
    }
    
    public async Task<Tenant?> GetByTelegramIdAsync(long telegramId)
    {
        const string sql = @"
            SELECT 
                id, email, full_name, phone, 
                telegram_id, preferred_channel, is_blocked, created_at
            FROM tenants 
            WHERE telegram_id = @TelegramId";

        var dto = await _unitOfWork.Connection!
            .QueryFirstOrDefaultAsync<TenantDto>(sql, new { TelegramId = telegramId });
        
        return dto == null ? null : MapToTenant(dto);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        const string sql = @"
            SELECT 
                id, email, full_name, phone, 
                telegram_id, preferred_channel, is_blocked, created_at
            FROM tenants 
            ORDER BY created_at DESC";

        var dtos = await _unitOfWork.Connection!
            .QueryAsync<TenantDto>(sql);
    
        return dtos.Select(MapToTenant);
    }

    public async Task AddAsync(Tenant tenant)
    {
        const string sql = @"
            INSERT INTO tenants (
                id, email, full_name, phone, 
                telegram_id, preferred_channel, is_blocked, created_at
            ) VALUES (
                @Id, @Email, @FullName, @Phone, 
                @TelegramId, @PreferredChannel, @IsBlocked, @CreatedAt
            )";

        await _unitOfWork.Connection!.ExecuteAsync(sql, new
        {
            tenant.Id,
            tenant.Email,
            tenant.Contact.FullName,
            tenant.Contact.Phone,
            tenant.TelegramId,
            PreferredChannel = (int)tenant.PreferredChannel,
            tenant.IsBlocked,
            tenant.CreatedAt
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task UpdateAsync(Tenant tenant)
    {
        const string sql = @"
            UPDATE tenants SET 
                email = @Email,
                full_name = @FullName,
                phone = @Phone,
                telegram_id = @TelegramId,
                preferred_channel = @PreferredChannel,
                is_blocked = @IsBlocked
            WHERE id = @Id";

        await _unitOfWork.Connection!.ExecuteAsync(sql, new
        {
            tenant.Id,
            tenant.Email,
            tenant.Contact.FullName,
            tenant.Contact.Phone,
            tenant.TelegramId,
            PreferredChannel = (int)tenant.PreferredChannel,
            tenant.IsBlocked
        }, transaction: _unitOfWork.Transaction);
    }
    
    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM tenants WHERE id = @Id";
        await _unitOfWork.Connection!.ExecuteAsync(
            sql, 
            new { Id = id }, 
            transaction: _unitOfWork.Transaction);
    }
}