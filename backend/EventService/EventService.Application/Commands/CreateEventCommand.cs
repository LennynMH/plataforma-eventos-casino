using MediatR;
using EventService.Application.DTOs;

namespace EventService.Application.Commands;

public class CreateEventCommand : IRequest<EventResponse>
{
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<ZoneRequest> Zones { get; set; } = new();
}
