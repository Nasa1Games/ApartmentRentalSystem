using ApartmentRentalSystem.Application.Dto;
using ApartmentRentalSystem.Application.Interfaces;
using MediatR;

namespace ApartmentRentalSystem.Application.Queries.Bookings;

public record GetFilteredBookingsQuery(
    int? Entrance = null,
    int? Floor = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int? MinCapacity = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    string? Status = null
) : IRequest<IEnumerable<FilteredBookingDto>>;


public class GetFilteredBookingsQueryHandler : IRequestHandler<GetFilteredBookingsQuery, IEnumerable<FilteredBookingDto>>
{
    private readonly IBookingQueryRepository _queryRepository;

    public GetFilteredBookingsQueryHandler(IBookingQueryRepository queryRepository) 
        => _queryRepository = queryRepository;

    public async Task<IEnumerable<FilteredBookingDto>> Handle(
        GetFilteredBookingsQuery request, CancellationToken cancellationToken)
        => await _queryRepository.GetFilteredBookingsAsync(
            request.Entrance,
            request.Floor,
            request.FromDate,
            request.ToDate,
            request.MinCapacity,
            request.MinPrice,
            request.MaxPrice,
            request.Status);
}