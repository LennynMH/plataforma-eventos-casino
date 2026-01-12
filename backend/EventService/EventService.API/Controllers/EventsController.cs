using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using EventService.Application.Commands;
using EventService.Application.Queries;
using EventService.Application.DTOs;
using EventService.Application.Validators;
using EventService.Infrastructure.Caching;
using FluentValidation;

namespace EventService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRedisCacheService _cache;
    private readonly IValidator<CreateEventRequest> _validator;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IMediator mediator,
        IRedisCacheService cache,
        IValidator<CreateEventRequest> validator,
        ILogger<EventsController> logger)
    {
        _mediator = mediator;
        _cache = cache;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Crear un nuevo evento (requiere rol Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<EventResponse>> CreateEvent([FromBody] CreateEventRequest request)
    {
        // Validar
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        try
        {
            var command = new CreateEventCommand
            {
                Name = request.Name,
                Date = request.Date,
                Location = request.Location,
                Zones = request.Zones
            };

            var result = await _mediator.Send(command);

            // Invalidar cache
            await _cache.RemoveByPatternAsync("events:list:*");

            return CreatedAtAction(nameof(GetEventById), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear evento");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Listar eventos (requiere rol User o Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "User")]
    public async Task<ActionResult<List<EventResponse>>> GetEvents([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            // Intentar obtener del cache
            var cacheKey = $"events:list:{page}:{pageSize}";
            var cachedEvents = await _cache.GetAsync<List<EventResponse>>(cacheKey);
            
            if (cachedEvents != null)
            {
                return Ok(cachedEvents);
            }

            // Si no está en cache, obtener de la BD
            var query = new GetEventsQuery { Page = page, PageSize = pageSize };
            var events = await _mediator.Send(query);

            // Guardar en cache (TTL: 5 minutos)
            await _cache.SetAsync(cacheKey, events, TimeSpan.FromMinutes(5));

            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar eventos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener detalle de un evento por ID (requiere rol User o Admin)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "User")]
    public async Task<ActionResult<EventResponse>> GetEventById(Guid id)
    {
        try
        {
            var query = new GetEventByIdQuery { Id = id };
            var eventResponse = await _mediator.Send(query);

            if (eventResponse == null)
            {
                return NotFound();
            }

            return Ok(eventResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener evento {EventId}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
