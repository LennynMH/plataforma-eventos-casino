using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<CreateEventCommandHandler> _logger;

    public CreateEventCommandHandler(
        EventDbContext context,
        IMapper mapper,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateEventCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
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
        
        _logger.LogInformation(
            "Evento guardado en BD: Id={EventId}, Name={EventName}", 
            eventEntity.Id, eventEntity.Name);

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

        _logger.LogInformation(
            "Publicando mensaje EventCreated a RabbitMQ: MessageId={MessageId}, EventId={EventId}, Name={Name}, Exchange=EventCreated",
            eventCreated.MessageId, eventCreated.EventId, eventCreated.Name);

        // Intentar publicar a RabbitMQ de forma asíncrona y no bloqueante
        // El evento ya está guardado en BD, así que la operación principal fue exitosa
        // Usar Publish() que publica al exchange "EventCreated" (configurado con SetEntityName)
        // NotificationService tiene configurado el consumer que escucha la cola y está bindeada al exchange
        _ = Task.Run(async () =>
        {
            try
            {
                // Agregar timeout para evitar que se quede colgado
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                
                // Publicar al exchange "EventCreated" (no intenta declarar colas)
                await _publishEndpoint.Publish(eventCreated, cts.Token);
                
                _logger.LogInformation(
                    "Mensaje publicado exitosamente a RabbitMQ: MessageId={MessageId}, EventId={EventId}",
                    eventCreated.MessageId, eventCreated.EventId);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    "Timeout al publicar mensaje a RabbitMQ (el evento fue guardado en BD): MessageId={MessageId}, EventId={EventId}",
                    eventCreated.MessageId, eventCreated.EventId);
            }
            catch (Exception ex)
            {
                // Log del error pero NO lanzar excepción para no bloquear la respuesta
                // El evento ya está guardado en BD, así que la operación principal fue exitosa
                _logger.LogWarning(ex, 
                    "No se pudo publicar mensaje a RabbitMQ (el evento fue guardado en BD): MessageId={MessageId}, EventId={EventId}. " +
                    "Error: {ErrorMessage}",
                    eventCreated.MessageId, eventCreated.EventId, ex.Message);
            }
        });

        // Retornar respuesta inmediatamente (el evento ya está guardado en BD)
        // El envío a RabbitMQ se hace en background
        return _mapper.Map<EventResponse>(eventEntity);
    }
}
