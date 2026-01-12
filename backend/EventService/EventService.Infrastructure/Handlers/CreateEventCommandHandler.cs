using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EventService.Application.Commands;
using EventService.Application.DTOs;
using EventService.Domain.Entities;
using EventService.Domain.Events;
using EventService.Infrastructure.Data;
using MassTransit;

namespace EventService.Infrastructure.Handlers;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, EventResponse>
{
    private readonly EventDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateEventCommandHandler(
        EventDbContext context,
        IMapper mapper,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<EventResponse> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        // Crear evento
        var eventEntity = _mapper.Map<Domain.Entities.Event>(request);
        
        // Mapear zonas
        var zones = request.Zones.Select(z => _mapper.Map<Zone>(z)).ToList();
        foreach (var zone in zones)
        {
            zone.EventId = eventEntity.Id;
        }
        eventEntity.Zones = zones;

        // Guardar en transacción
        await _context.Events.AddAsync(eventEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Publicar evento al broker
        var eventCreated = new EventCreated
        {
            MessageId = Guid.NewGuid(),
            EventId = eventEntity.Id,
            Name = eventEntity.Name,
            OccurredAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            Version = 1
        };

        await _publishEndpoint.Publish(eventCreated, cancellationToken);

        // Retornar respuesta
        return _mapper.Map<EventResponse>(eventEntity);
    }
}
