using System.Text;

namespace EventService.API.Middleware;

public class SensitiveDataLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SensitiveDataLoggingMiddleware> _logger;

    public SensitiveDataLoggingMiddleware(
        RequestDelegate next,
        ILogger<SensitiveDataLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        var headers = context.Request.Headers
            .Where(h => !IsSensitiveHeader(h.Key))
            .ToDictionary(h => h.Key, h => h.Value.ToString());

        _logger.LogInformation(
            "Request: {Method} {Path} | Response: {StatusCode}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode);

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static bool IsSensitiveHeader(string headerName)
    {
        var sensitiveHeaders = new[]
        {
            "Authorization",
            "X-API-Key",
            "Cookie",
            "X-Auth-Token",
            "Password",
            "password",
            "token",
            "Token"
        };

        return sensitiveHeaders.Any(h => 
            headerName.Contains(h, StringComparison.OrdinalIgnoreCase));
    }

    private static string SanitizeValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        // Reemplazar tokens, passwords, etc. con [REDACTED]
        if (value.Length > 20 && (value.Contains("Bearer", StringComparison.OrdinalIgnoreCase) ||
                                  value.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                                  value.Contains("password", StringComparison.OrdinalIgnoreCase)))
        {
            return "[REDACTED]";
        }

        return value;
    }
}
