using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace EventService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Login simple para generar JWT (solo para MVP/demo)
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Validación básica (para MVP)
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Username y password son requeridos" });
        }

        // Para el MVP, aceptamos credenciales simples
        // En producción, esto debería validar contra una BD de usuarios con hash de passwords
        
        var role = request.Username?.ToLower() == "admin" ? "Admin" : "User";
        
        // Log sin datos sensibles
        _logger.LogInformation("Intento de login para usuario: {Username} (rol: {Role})", 
            request.Username, role);
        
        var token = GenerateJwtToken(request.Username ?? "user", role);
        
        // No loguear el token generado
        return Ok(new { token });
    }

    private string GenerateJwtToken(string username, string role)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345678901234567890";
        var issuer = jwtSettings["Issuer"] ?? "EventService";
        var audience = jwtSettings["Audience"] ?? "EventService";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim("role", role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Obtener tiempo de expiración desde configuración (default: 24 horas)
        var expirationHours = int.TryParse(jwtSettings["ExpirationHours"], out var hours) 
            ? hours 
            : 24;
        
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expirationHours),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
