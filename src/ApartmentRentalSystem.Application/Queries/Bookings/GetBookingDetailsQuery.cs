using ApartmentRentalSystem.Application.Dto;
using ApartmentRentalSystem.Application.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Bookings;

// Команда -- контейнер данных, поэтому должна быть иммутабельной.
// IRequest<...> означает что команда вернет IEnumerable<BookingDetailsDto>
public record GetBookingDetailsQuery(Guid BookingId) : IRequest<BookingDetailsDto?>;


public class GetBookingDetailsQueryHandler : IRequestHandler<GetBookingDetailsQuery, BookingDetailsDto?>
{
    private readonly IBookingQueryRepository _queryRepository;

    public GetBookingDetailsQueryHandler(IBookingQueryRepository queryRepository) 
        => _queryRepository = queryRepository;

    public async Task<BookingDetailsDto?> Handle(
        GetBookingDetailsQuery request, CancellationToken cancellationToken) 
        => await _queryRepository.GetBookingDetailsAsync(request.BookingId);
}