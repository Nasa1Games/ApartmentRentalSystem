using System.Data;
using System.Text;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.Specifications;
using ApartmentRentalSystem.Core.ValueObjects;
using ApartmentRentalSystem.Infrastructure.Persistence.Db;
using Dapper;
using Npgsql;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Repositories;

public class PostgresApartmentRepository : IApartmentRepository
{
    private readonly IDbConnectionFactory _factory;

    public PostgresApartmentRepository(IDbConnectionFactory factory) => _factory = factory;

    private static Apartment MapToApartment(dynamic row)
    {
        return new Apartment(
            id: row.id,
            unitNumber: row.unit_number.ToString(),
            entrance: row.entrance,
            floor: row.floor,
            capacity: row.capacity,
            basePricePerNight: new Money((decimal)row.base_price_per_night),
            description: row.description.ToString(),
            status: Enum.Parse<ApartmentStatus>(row.status.ToString(), ignoreCase: true),
            createdAt: row.created_at is DateTime dt
                ? dt
                : DateTime.Parse(row.created_at.ToString()
                ));
    }
    
    public async Task<Apartment?> GetByIdAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            unit_number, 
            entrance, 
            floor,
            capacity, 
            base_price_per_night, 
            description,
            status, 
            created_at
        FROM apartments WHERE id = @Id";
        
        var row = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id }); 
        return row == null ? null : MapToApartment(row);
    }

    public async Task<IEnumerable<Apartment>> GetAllAsync()
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
        SELECT 
            id, 
            unit_number, 
            entrance, 
            floor,
            capacity, 
            base_price_per_night, 
            description,
            status, 
            created_at
        FROM apartments ORDER BY entrance, floor, unit_number";
        
        var rows = await conn.QueryAsync(sql);
        return rows.Select(MapToApartment);
    }

    public async Task<IEnumerable<Apartment>> GetFilteredAsync(ApartmentFilterCriteria criteria)
    {
        using var conn = _factory.CreateConnection();
        var sql = new StringBuilder(@"
        SELECT 
            id, 
            unit_number, 
            entrance, 
            floor,
            capacity, 
            base_price_per_night, 
            description,
            status, 
            created_at
        FROM apartments WHERE 1=1");

        if (criteria.MinFloor.HasValue) sql.Append(" AND floor >= @MinFloor");
        if (criteria.MaxFloor.HasValue) sql.Append(" AND floor <= @MaxFloor");
        if (criteria.MinCapacity.HasValue) sql.Append(" AND capacity >= @MinCapacity");
        if (!string.IsNullOrEmpty(criteria.Status)) sql.Append(" AND status = @Status");

        sql.Append(" ORDER BY entrance, floor, unit_number");
        var row = await conn.QueryAsync<Apartment>(sql.ToString(), new
        {
            criteria.MinFloor, criteria.MaxFloor, criteria.MinCapacity, criteria.Status
        });
        return row.Select(MapToApartment);
    }

    public async Task AddAsync(Apartment apartment)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            INSERT INTO apartments (id, unit_number, entrance, floor, capacity, base_price_per_night, description, status, created_at)
            VALUES (@Id, @UnitNumber, @Entrance, @Floor, @Capacity, @BasePricePerNight, @Description, @Status, @CreatedAt)";
        
        await conn.ExecuteAsync(sql, new
        {
            apartment.Id,
            apartment.UnitNumber,
            apartment.Entrance,
            apartment.Floor,
            apartment.Capacity,
            BasePricePerNight = apartment.BasePricePerNight.Amount,
            apartment.Description,
            Status = apartment.Status.ToString(),
            apartment.CreatedAt
        });
    }

    public async Task UpdateAsync(Apartment apartment)
    {
        using var conn = _factory.CreateConnection();
        const string sql = @"
            UPDATE apartments SET 
                entrance = @Entrance, floor = @Floor, capacity = @Capacity, 
                base_price_per_night = @BasePricePerNight, description = @Description, status = @Status
            WHERE id = @Id";

        await conn.ExecuteAsync(sql, new
        {
            apartment.Id,
            apartment.Entrance, 
            apartment.Floor, 
            apartment.Capacity,
            BasePricePerNight = apartment.BasePricePerNight.Amount,
            apartment.Description, 
            Status = apartment.Status.ToString()
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync("DELETE FROM apartments WHERE id = @Id", new { Id = id });
    }
}