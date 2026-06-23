using Npgsql;

namespace ApartmentRentalSystem.Infrastructure.Db.Connection;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}