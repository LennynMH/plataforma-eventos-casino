using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EventService.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        object response;

        // Determinar el tipo de excepción y establecer el código de estado apropiado
        switch (exception)
        {
            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Error de validación",
                    message = "Los datos proporcionados no son válidos",
                    statusCode = context.Response.StatusCode,
                    timestamp = DateTime.UtcNow,
                    details = validationEx.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        error = e.ErrorMessage
                    })
                };
                _logger.LogWarning(exception, 
                    "Error de validación: {Message}. Path: {Path}", 
                    exception.Message, 
                    context.Request.Path);
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new
                {
                    error = "No autorizado",
                    message = "No tiene permisos para realizar esta acción",
                    statusCode = context.Response.StatusCode,
                    timestamp = DateTime.UtcNow
                };
                _logger.LogWarning(exception, 
                    "Error de autorización: {Message}. Path: {Path}", 
                    exception.Message, 
                    context.Request.Path);
                break;

            case KeyNotFoundException:
            case ArgumentNullException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new
                {
                    error = "Recurso no encontrado",
                    message = exception.Message,
                    statusCode = context.Response.StatusCode,
                    timestamp = DateTime.UtcNow,
                    details = (object?)null
                };
                _logger.LogWarning(exception, 
                    "Recurso no encontrado: {Message}. Path: {Path}", 
                    exception.Message, 
                    context.Request.Path);
                break;

            case DbUpdateException dbEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Error en la base de datos",
                    message = "No se pudo completar la operación en la base de datos",
                    statusCode = context.Response.StatusCode,
                    timestamp = DateTime.UtcNow,
                    details = _environment.IsDevelopment() 
                        ? dbEx.InnerException?.Message 
                        : null
                };
                _logger.LogError(exception, 
                    "Error de base de datos: {Message}. Path: {Path}", 
                    exception.Message, 
                    context.Request.Path);
                break;

            case ArgumentException:
            case InvalidOperationException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    error = "Solicitud inválida",
                    message = exception.Message,
                    statusCode = context.Response.StatusCode,
                    timestamp = DateTime.UtcNow,
                    details = (object?)null
                };
                _logger.LogWarning(exception, 
                    "Solicitud inválida: {Message}. Path: {Path}", 
                    exception.Message, 
                    context.Request.Path);
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new
                {
                    error = "Ha ocurrido un error interno del servidor",
                    message = _environment.IsDevelopment() 
                        ? exception.Message 
                        : "Por favor, contacte al administrador del sistema",
                    statusCode = context.Response.StatusCode,
                    timestamp = DateTime.UtcNow,
                    details = _environment.IsDevelopment() 
                        ? exception.StackTrace 
                        : null
                };
                _logger.LogError(exception, 
                    "Error no manejado: {Message}. Path: {Path}. StackTrace: {StackTrace}", 
                    exception.Message, 
                    context.Request.Path,
                    exception.StackTrace);
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
