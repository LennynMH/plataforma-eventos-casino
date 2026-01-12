using MediatR;
using EventService.Application.DTOs;

namespace EventService.Application.Queries;

public class GetEventByIdQuery : IRequest<EventResponse?>
{
    public Guid Id { get; set; }
}
