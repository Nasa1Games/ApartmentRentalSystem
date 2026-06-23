using ApartmentRentalSystem.Application.Dto;
using ApartmentRentalSystem.Application.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Bookings;

// Команда -- контейнер данных, поэтому должна быть иммутабельной.
// IRequest<...> означает что команда вернет IEnumerable<PendingBookingDto>
public record GetPendingBookingsQuery : IRequest<IEnumerable<PendingBookingDto>>;


public class GetPendingBookingsQueryHandler : IRequestHandler<GetPendingBookingsQuery, IEnumerable<PendingBookingDto>>
{
    private readonly IBookingQueryRepository _queryRepository;

    public GetPendingBookingsQueryHandler(IBookingQueryRepository queryRepository) 
        => _queryRepository = queryRepository;

    public async Task<IEnumerable<PendingBookingDto>> Handle(
        GetPendingBookingsQuery request, CancellationToken cancellationToken) 
        => await _queryRepository.GetPendingBookingsAsync();
}