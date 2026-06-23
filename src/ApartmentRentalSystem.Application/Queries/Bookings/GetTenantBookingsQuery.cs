using ApartmentRentalSystem.Application.Dto;
using ApartmentRentalSystem.Application.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Bookings;

// Команда -- контейнер данных, поэтому должна быть иммутабельной.
// IRequest<...> означает что команда вернет IEnumerable<UserBookingDto>
public record GetTenantBookingsQuery(Guid TenantId) : IRequest<IEnumerable<UserBookingDto>>;


public class GetTenantBookingsQueryHandler : IRequestHandler<GetTenantBookingsQuery, IEnumerable<UserBookingDto>>
{
    private readonly IBookingQueryRepository _queryRepository;

    public GetTenantBookingsQueryHandler(IBookingQueryRepository queryRepository) 
        => _queryRepository = queryRepository;

    public async Task<IEnumerable<UserBookingDto>> Handle(
        GetTenantBookingsQuery request, CancellationToken cancellationToken) 
        => await _queryRepository.GetBookingsByTenantIdAsync(request.TenantId);
}