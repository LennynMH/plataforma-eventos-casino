using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EventService.Application.Queries;
using EventService.Application.DTOs;
using EventService.Infrastructure.Data;

namespace EventService.Infrastructure.Handlers;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, List<EventResponse>>
{
    private readonly EventDbContext _context;
    private readonly IMapper _mapper;

    public GetEventsQueryHandler(EventDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EventResponse>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _context.Events
            .Include(e => e.Zones)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EventResponse>>(events);
    }
}
