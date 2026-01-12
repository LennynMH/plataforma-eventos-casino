namespace NotificationService.Domain.Entities;

public class ProcessedMessage
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; } // Para idempotencia
    public DateTime ProcessedAt { get; set; }
    public string Status { get; set; } = "Processed"; // Processed, Failed
}
