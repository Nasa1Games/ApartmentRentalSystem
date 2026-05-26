using Npgsql;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Db;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}