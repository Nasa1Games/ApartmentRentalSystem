using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Infrastructure.Persistence.Db;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Repositories;

public class PostgresTenantRepository(IDbConnectionFactory factory) : ITenantRepository
{
    private static Tenant MapToTenant(dynamic row)
    {
        //Console.WriteLine(row);
        return new Tenant(
            id: row.id,
            email: row.email,
            passwordHash: row.password,
            contact: new ContactInfo(
                fullName: row.full_name,
                email: row.email,
                phone: row.phone),
            userRole: Enum.Parse<UserRole>(row.role),
            telegramId: row.telegram_id,
            isBlocked: row.is_blocked,
            createdAt: row.created_at
            );
    }

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
        await using var conn = factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            email, 
            password,
            full_name, 
            phone, 
            role,
            telegram_id,
            is_blocked, 
            created_at
        FROM users WHERE id = @Id";
    
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });
        return row == null ? null : MapToTenant(row);
    }


    public async Task<Tenant?> GetByEmailAsync(string email)
    {
        await using var conn = factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            email, 
            password,
            full_name, 
            phone, 
            role,
            telegram_id,
            is_blocked, 
            created_at
        FROM users WHERE email = @Email";
    
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Email = email });
        return row == null ? null : MapToTenant(row);
    }
    
    public async Task<Tenant?> GetByTelegramIdAsync(long telegramId)
    {
        await using var conn = factory.CreateConnection();
        const string sql = @"
        SELECT
            id, 
            email, 
            password, 
            full_name, 
            phone, 
            role, 
            telegram_id,
            is_blocked, 
            created_at
        FROM users 
        WHERE telegram_id = @TelegramId";
        
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { TelegramId = telegramId });
        return row == null ? null : MapToTenant(row);
    }

    public async Task<IEnumerable<Tenant>> GetAllAsync()
    {
        await using var conn = factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            email, 
            password,
            full_name, 
            phone, 
            role,
            telegram_id,
            is_blocked, 
            created_at
        FROM users ORDER BY created_at DESC";
    
        var rows = await conn.QueryAsync(sql);
        return rows.Select(MapToTenant);
    }

    public async Task AddAsync(Tenant tenant)
    {
        await using var conn = factory.CreateConnection();
        const string sql = @"
            INSERT INTO users (id, email, password, full_name, phone, role, telegram_id, is_blocked, created_at)
            VALUES (@Id, @Email, @PasswordHash, @FullName, @Phone, @Role, @TelegramId, @IsBlocked, @CreatedAt)";
        
        await conn.ExecuteAsync(sql, new
        {
            tenant.Id,
            tenant.Email,
            tenant.PasswordHash,
            tenant.Contact.FullName,
            tenant.Contact.Phone,
            Role = tenant.UserRole.ToString(),
            tenant.TelegramId,
            tenant.IsBlocked,
            tenant.CreatedAt
        });
    }

    public async Task UpdateTelegramIdAsync(Guid id, long telegramId)
    {
        await using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE users SET telegram_id = @TelegramId WHERE id = @Id", 
            new { Id = id, TelegramId = telegramId });
    }
    
    public async Task UpdateAsync(Tenant tenant)
    {
        await using var conn = factory.CreateConnection();
        const string sql = @"
            UPDATE users SET 
                email = @Email,
                full_name = @FullName,
                phone = @Phone,
                role = @Role,
                telegram_id = @TelegramId,
                is_blocked = @IsBlocked
            WHERE id = @Id";
        
        await conn.ExecuteAsync(sql, new
        {
            tenant.Id,
            tenant.Email,
            tenant.Contact.FullName,
            tenant.Contact.Phone,
            Role = tenant.UserRole.ToString(),
            tenant.TelegramId,
            tenant.IsBlocked
        });
    }

    public async Task BlockAsync(Guid id)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE users SET is_blocked = true WHERE id = @Id", 
            new { Id = id });
    }

    public async Task UnblockAsync(Guid id)
    {
        using var conn = factory.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE users SET is_blocked = false WHERE id = @Id", 
            new { Id = id });
    }
}