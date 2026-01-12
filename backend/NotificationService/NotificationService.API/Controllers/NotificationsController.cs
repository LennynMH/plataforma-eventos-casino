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
    public async Task<ActionResult> GetNotificationLogs(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        try
        {
            var query = _context.NotificationLogs.AsQueryable();

            // Filtro por status si se proporciona
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(n => n.Status == status);
            }

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                logs
            });
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

    /// <summary>
    /// Listar notificaciones fallidas con filtros opcionales
    /// </summary>
    [HttpGet("failed")]
    public async Task<ActionResult> GetFailedNotifications(
        [FromQuery] Guid? eventId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.NotificationLogs
                .Where(n => n.Status == "Failed");

            // Filtro por EventId
            if (eventId.HasValue)
            {
                query = query.Where(n => n.EventId == eventId.Value);
            }

            // Filtro por rango de fechas
            if (startDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(n => n.CreatedAt <= endDate.Value);
            }

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                total,
                page,
                pageSize,
                failed = logs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar notificaciones fallidas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener estadísticas de notificaciones fallidas
    /// </summary>
    [HttpGet("failed/statistics")]
    public async Task<ActionResult> GetFailedStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7); // Últimos 7 días por defecto
            var end = endDate ?? DateTime.UtcNow;

            var total = await _context.NotificationLogs
                .Where(n => n.CreatedAt >= start && n.CreatedAt <= end)
                .CountAsync();

            var failed = await _context.NotificationLogs
                .Where(n => n.Status == "Failed" && n.CreatedAt >= start && n.CreatedAt <= end)
                .CountAsync();

            var success = await _context.NotificationLogs
                .Where(n => n.Status == "Sent" && n.CreatedAt >= start && n.CreatedAt <= end)
                .CountAsync();

            var successRate = total > 0 ? (double)success / total * 100 : 0;

            // Agrupar por tipo de error
            var errorTypes = await _context.NotificationLogs
                .Where(n => n.Status == "Failed" && n.CreatedAt >= start && n.CreatedAt <= end && n.ErrorMessage != null)
                .GroupBy(n => n.ErrorMessage)
                .Select(g => new { Error = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return Ok(new
            {
                period = new { start, end },
                total,
                failed,
                success,
                successRate = Math.Round(successRate, 2),
                topErrors = errorTypes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de notificaciones fallidas");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}
