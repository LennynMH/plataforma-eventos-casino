using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Email;
using Polly;
using Polly.Retry;

namespace NotificationService.Infrastructure.Consumers;

// Contrato del mensaje (debe coincidir con EventService)
public class EventCreatedMessage
{
    public Guid MessageId { get; set; }
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public Guid CorrelationId { get; set; }
    public int Version { get; set; } = 1;
}

public class EventCreatedConsumer : IConsumer<EventCreatedMessage>
{
    private readonly NotificationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<EventCreatedConsumer> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public EventCreatedConsumer(
        NotificationDbContext context,
        IEmailService emailService,
        ILogger<EventCreatedConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;

        // Configurar política de reintentos con Polly
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Reintento {RetryCount} después de {TimeSpan}s. Error: {Error}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });
    }

    public async Task Consume(ConsumeContext<EventCreatedMessage> context)
    {
        var message = context.Message;
        
        _logger.LogInformation(
            "Consumiendo EventCreated: MessageId={MessageId}, EventId={EventId}, Name={Name}",
            message.MessageId, message.EventId, message.Name);

        // Verificar idempotencia
        var alreadyProcessed = await _context.ProcessedMessages
            .AnyAsync(p => p.MessageId == message.MessageId);

        if (alreadyProcessed)
        {
            _logger.LogWarning(
                "Mensaje {MessageId} ya fue procesado. Saltando procesamiento (idempotencia).",
                message.MessageId);
            return; // ACK sin procesar
        }

        // Calcular hash del payload para auditoría
        var payloadJson = JsonSerializer.Serialize(message);
        var payloadHash = ComputeHash(payloadJson);

        // Crear registro de notificación
        var notificationLog = new NotificationLog
        {
            Id = Guid.NewGuid(),
            EventId = message.EventId,
            EventName = message.Name,
            OccurredAt = message.OccurredAt,
            CorrelationId = message.CorrelationId,
            PayloadHash = payloadHash,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _context.NotificationLogs.AddAsync(notificationLog);

        // Registrar mensaje como procesado (para idempotencia)
        var processedMessage = new ProcessedMessage
        {
            Id = Guid.NewGuid(),
            MessageId = message.MessageId,
            ProcessedAt = DateTime.UtcNow,
            Status = "Processed"
        };

        await _context.ProcessedMessages.AddAsync(processedMessage);
        await _context.SaveChangesAsync();

        // Enviar email con reintentos
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                // Para el MVP, usamos datos básicos del mensaje
                // En producción, podríamos consultar EventService para más detalles
                await _emailService.SendEventCreatedNotificationAsync(
                    message.EventId,
                    message.Name,
                    DateTime.UtcNow, // Usar fecha del mensaje o consultar EventService
                    "Lugar no disponible en mensaje"); // En producción, consultar EventService
            });

            // Actualizar estado a Sent
            notificationLog.Status = "Sent";
            notificationLog.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Notificación procesada exitosamente para evento {EventId}",
                message.EventId);
        }
        catch (Exception ex)
        {
            // Actualizar estado a Failed
            notificationLog.Status = "Failed";
            notificationLog.ErrorMessage = ex.Message;
            notificationLog.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogError(ex,
                "Error al procesar notificación para evento {EventId}. El mensaje será enviado a DLQ.",
                message.EventId);

            // Re-lanzar excepción para que MassTransit envíe a DLQ
            throw;
        }
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
