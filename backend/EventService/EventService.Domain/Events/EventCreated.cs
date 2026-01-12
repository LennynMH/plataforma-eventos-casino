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
