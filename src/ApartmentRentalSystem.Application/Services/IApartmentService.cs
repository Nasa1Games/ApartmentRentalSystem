using ApartmentRentalSystem.Application.DTOs;

namespace ApartmentRentalSystem.Application.Services;

public interface IApartmentService
{
    Task<IEnumerable<ApartmentListItemDto>> GetAvailableApartmentsAsync(CancellationToken ct = default);
}