namespace EventService.Domain.Entities;

public class Zone
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation property
    public Event? Event { get; set; }
}
