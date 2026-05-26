using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ApartmentRentalSystem.Infrastructure.Persistence.Db;

public class PostgresConnectionFactory: IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnection")
                            ?? throw new InvalidOperationException("Connection string 'PostgresConnection' not found.");
    }

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}