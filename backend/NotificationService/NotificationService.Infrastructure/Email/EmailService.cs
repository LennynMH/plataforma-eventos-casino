using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace NotificationService.Infrastructure.Email;

public interface IEmailService
{
    Task SendEventCreatedNotificationAsync(Guid eventId, string eventName, DateTime eventDate, string location);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEventCreatedNotificationAsync(Guid eventId, string eventName, DateTime eventDate, string location)
    {
        try
        {
            var emailConfig = _configuration.GetSection("Email");
            var smtpHost = emailConfig["SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(emailConfig["SmtpPort"] ?? "587");
            var smtpUsername = emailConfig["SmtpUsername"] ?? string.Empty;
            var smtpPassword = emailConfig["SmtpPassword"] ?? string.Empty;
            var fromEmail = emailConfig["FromEmail"] ?? "noreply@eventplatform.com";
            var fromName = emailConfig["FromName"] ?? "Plataforma de Eventos";

            // Crear mensaje
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("Administrador", smtpUsername)); // Para MVP, enviar a admin
            message.Subject = $"Nuevo Evento Creado: {eventName}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Nuevo Evento Creado</h2>
                    <p><strong>ID del Evento:</strong> {eventId}</p>
                    <p><strong>Nombre:</strong> {eventName}</p>
                    <p><strong>Fecha:</strong> {eventDate:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Lugar:</strong> {location}</p>
                    <p>El evento ha sido creado exitosamente en la plataforma.</p>
                "
            };

            message.Body = bodyBuilder.ToMessageBody();

            // Enviar email
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUsername, smtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email enviado exitosamente para evento {EventId}", eventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email para evento {EventId}", eventId);
            throw;
        }
    }
}
