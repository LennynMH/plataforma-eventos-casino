// Este archivo define el tipo EventCreated con el mismo namespace completo que EventService
// Esto permite que MassTransit haga match del tipo completo y enrute correctamente los mensajes
// desde EventService hacia NotificationService
namespace EventService.Domain.Events;

public class EventCreated
{
    public Guid MessageId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public Guid CorrelationId { get; set; }
    public int Version { get; set; } = 1;
}
