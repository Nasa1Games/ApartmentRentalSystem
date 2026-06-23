using System.Data;

namespace ApartmentRentalSystem.Core.Interfaces;

public interface INotificationDbConnectionFactory
{
    IDbConnection CreateConnection();
}