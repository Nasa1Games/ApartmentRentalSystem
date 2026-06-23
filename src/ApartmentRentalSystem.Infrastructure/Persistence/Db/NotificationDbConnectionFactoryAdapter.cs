using System.Data;
using ApartmentRentalSystem.Core.Interfaces;

namespace ApartmentRentalSystem.Infrastructure.Db.Connection;

public class NotificationDbConnectionFactoryAdapter : INotificationDbConnectionFactory
{
    private readonly IDbConnectionFactory _factory;

    public NotificationDbConnectionFactoryAdapter(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IDbConnection CreateConnection()
    {
        return _factory.CreateConnection();
    }
}