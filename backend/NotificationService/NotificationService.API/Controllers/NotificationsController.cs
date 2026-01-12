using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Infrastructure.Data;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        NotificationDbContext context,
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Listar logs de notificaciones (para debugging/monitoreo)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetNotificationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var logs = await _context.NotificationLogs
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar logs de notificaciones");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener log de notificación por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetNotificationLog(Guid id)
    {
        try
        {
            var log = await _context.NotificationLogs
                .FirstOrDefaultAsync(n => n.Id == id);

            if (log == null)
            {
                return NotFound();
            }

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener log de notificación {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
