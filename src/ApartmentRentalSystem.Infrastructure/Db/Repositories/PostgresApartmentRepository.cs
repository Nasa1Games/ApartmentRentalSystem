using System.Text;
using ApartmentRentalSystem.Core.Entities;
using ApartmentRentalSystem.Core.Enums;
using ApartmentRentalSystem.Core.FilterCriteria;
using ApartmentRentalSystem.Core.Interfaces;
using ApartmentRentalSystem.Core.ValueObjects;
using Dapper;

namespace ApartmentRentalSystem.Infrastructure.Db.Repositories;

public class PostgresApartmentRepository : IApartmentRepository
{
    /// <summary>
    /// DTO для маппинга из БД
    /// </summary>
    private class ApartmentDto
    {
        public Guid id { get; set; }
        public string unit_number { get; set; } = null!;
        public int entrance { get; set; }
        public int floor { get; set; }
        public int capacity { get; set; }
        public decimal base_price_per_night { get; set; }
        public string description { get; set; } = null!;
        public string status { get; set; } = null!;
        public DateTime created_at { get; set; }
    }
    
    /// <summary>
    /// Объект с подключением, управлением транзакциями и списком событий
    /// </summary>
    private readonly IUnitOfWork _unitOfWork;

    public PostgresApartmentRepository(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;
    
    /// <summary>
    /// Функция для маппинга из DTO -> Domain Entity
    /// </summary>
    private static Apartment MapToApartment(ApartmentDto dto)
    {
        return new Apartment(
            id: dto.id,
            unitNumber: dto.unit_number,
            entrance: dto.entrance,
            floor: dto.floor,
            capacity: dto.capacity,
            basePricePerNight: new Money(dto.base_price_per_night),
            description: dto.description,
            status: Enum.Parse<ApartmentStatus>(dto.status, ignoreCase: true),
            createdAt: dto.created_at
        );
    }
    
    public async Task<Apartment?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT 
                id, unit_number, entrance, floor, 
                capacity, base_price_per_night, description, 
                status, created_at
            FROM apartments 
            WHERE id = @Id";

        var dto = await _unitOfWork.Connection!
            .QueryFirstOrDefaultAsync<ApartmentDto>(sql, new { Id = id });
        
        return dto == null ? null : MapToApartment(dto);
    }

    public async Task<IEnumerable<Apartment>> GetAllAsync()
    {
        const string sql = @"
            SELECT 
                id, unit_number, entrance, floor, 
                capacity, base_price_per_night, description, 
                status, created_at
            FROM apartments 
            ORDER BY entrance, floor, unit_number";
        
        var dtos = await _unitOfWork.Connection!
            .QueryAsync<ApartmentDto>(sql);
        
        return dtos.Select(MapToApartment);
    }

    public async Task<IEnumerable<Apartment>> GetFilteredAsync(ApartmentFilterCriteria criteria)
    {
        var sql = new StringBuilder(@"
            SELECT 
                id, unit_number, entrance, floor, 
                capacity, base_price_per_night, description, 
                status, created_at
            FROM apartments 
            WHERE 1=1");

        var parameters = new DynamicParameters();
        
        if (criteria.MinFloor.HasValue)
        {
            sql.Append(" AND floor >= @MinFloor");
            parameters.Add("MinFloor", criteria.MinFloor);
        }
        if (criteria.MaxFloor.HasValue)
        {
            sql.Append(" AND floor <= @MaxFloor");
            parameters.Add("MaxFloor", criteria.MaxFloor);
        }
        if (criteria.MinCapacity.HasValue)
        {
            sql.Append(" AND capacity >= @MinCapacity");
            parameters.Add("MinCapacity", criteria.MinCapacity);
        }
        if (criteria.Status.HasValue)
        {
            sql.Append(" AND status = @Status");
            parameters.Add("Status", criteria.Status.Value.ToString());
        }
        if (criteria.MinPrice != null)
        {
            sql.Append(" AND base_price_per_night >= @MinPrice");
            parameters.Add("MinPrice", criteria.MinPrice.Amount);
        }
        if (criteria.MaxPrice != null)
        {
            sql.Append(" AND base_price_per_night <= @MaxPrice");
            parameters.Add("MaxPrice", criteria.MaxPrice.Amount);
        }
        sql.Append(" ORDER BY entrance, floor, unit_number");
        
        var dtos = await _unitOfWork.Connection!
            .QueryAsync<ApartmentDto>(sql.ToString(), parameters);
        
        return dtos.Select(MapToApartment);
    }

    public async Task AddAsync(Apartment apartment)
    {
        const string sql = @"
            INSERT INTO apartments (id, unit_number, entrance, floor, capacity, base_price_per_night, description, status, created_at) 
            VALUES (@Id, @UnitNumber, @Entrance, @Floor, @Capacity, @BasePricePerNight, @Description, @Status, @CreatedAt)";

        await _unitOfWork.Connection!.ExecuteAsync(sql, new
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
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task UpdateAsync(Apartment apartment)
    {
        const string sql = @"
            UPDATE apartments SET 
                unit_number = @UnitNumber,
                entrance = @Entrance,
                floor = @Floor,
                capacity = @Capacity,
                base_price_per_night = @BasePricePerNight,
                description = @Description,
                status = @Status
            WHERE id = @Id";

        await _unitOfWork.Connection!.ExecuteAsync(sql, new
        {
            apartment.Id,
            apartment.UnitNumber,
            apartment.Entrance,
            apartment.Floor,
            apartment.Capacity,
            BasePricePerNight = apartment.BasePricePerNight.Amount,
            apartment.Description,
            Status = apartment.Status.ToString()
        }, transaction: _unitOfWork.Transaction);
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM apartments WHERE id = @Id";
        await _unitOfWork.Connection!.ExecuteAsync(
            sql, 
            new { Id = id }, 
            transaction: _unitOfWork.Transaction);
    }
}