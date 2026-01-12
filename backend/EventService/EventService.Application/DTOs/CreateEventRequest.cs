namespace EventService.Application.DTOs;

public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<ZoneRequest> Zones { get; set; } = new();
}

public class ZoneRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Capacity { get; set; }
}
