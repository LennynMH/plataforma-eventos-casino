using MediatR;
using EventService.Application.DTOs;

namespace EventService.Application.Queries;

public class GetEventsQuery : IRequest<List<EventResponse>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
