using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EventService.Application.Queries;
using EventService.Application.DTOs;
using EventService.Infrastructure.Data;

namespace EventService.Infrastructure.Handlers;

public class GetEventByIdQueryHandler : IRequestHandler<GetEventByIdQuery, EventResponse?>
{
    private readonly EventDbContext _context;
    private readonly IMapper _mapper;

    public GetEventByIdQueryHandler(EventDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<EventResponse?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .Include(e => e.Zones)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (eventEntity == null)
            return null;

        return _mapper.Map<EventResponse>(eventEntity);
    }
}
