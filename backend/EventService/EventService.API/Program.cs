using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using MassTransit;
using FluentValidation;
using MediatR;
using AutoMapper;
using EventService.Infrastructure.Data;
using EventService.Infrastructure.Caching;
using EventService.Application.Mappings;
using EventService.Application.Validators;
using EventService.Application.DTOs;
using EventService.Application.Commands;
using EventService.Infrastructure.Handlers;
using EventService.API.Middleware;
using EventService.Domain.Events;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Event Service API", Version = "v1" });
    
    // JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("EventDb");
builder.Services.AddDbContext<EventDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Configurar el esquema para la tabla de historial de migraciones
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "events");
        // Especificar el assembly donde están las migraciones
        var infrastructureAssembly = typeof(EventDbContext).Assembly;
        var assemblyName = infrastructureAssembly.GetName().Name;
        npgsqlOptions.MigrationsAssembly(assemblyName);
    });
});

// Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(redisConnection ?? "localhost:6379"));
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// MassTransit (RabbitMQ)
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "admin123");
        });
        
        // Configurar nombre de contrato personalizado para EventCreated
        // Esto asegura que el exchange tenga un nombre consistente "EventCreated"
        cfg.Message<EventCreated>(e =>
        {
            e.SetEntityName("EventCreated");
        });
    });
});

// MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommandHandler).Assembly);
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateEventRequestValidator>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JWT");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345678901234567890";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "EventService",
        ValidAudience = jwtSettings["Audience"] ?? "EventService",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    
    // Agregar eventos para debugging
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(context.Exception, "Error de autenticación JWT");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            var claims = context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}");
            logger.LogInformation("Token validado. Claims: {Claims}", string.Join(", ", claims ?? Array.Empty<string>()));
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Challenge JWT. Error: {Error}, ErrorDescription: {ErrorDescription}", 
                context.Error, context.ErrorDescription);
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // La política busca el claim "role" o el claim completo de Microsoft
    options.AddPolicy("Admin", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("role", "Admin") || 
        context.User.HasClaim(ClaimTypes.Role, "Admin") ||
        context.User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin")));
    options.AddPolicy("User", policy => policy.RequireAssertion(context =>
        context.User.HasClaim("role", "User") || 
        context.User.HasClaim("role", "Admin") ||
        context.User.HasClaim(ClaimTypes.Role, "User") ||
        context.User.HasClaim(ClaimTypes.Role, "Admin") ||
        context.User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "User") ||
        context.User.HasClaim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin")));
});

// Rate Limiting (Protección anti-abuso)
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.ClientIdHeader = "X-ClientId";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Period = "1m",
            Limit = 100 // 100 requests por minuto por IP
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/events",
            Period = "1m",
            Limit = 10 // 10 creaciones de eventos por minuto
        },
        new RateLimitRule
        {
            Endpoint = "POST:/api/auth/login",
            Period = "1m",
            Limit = 5 // 5 intentos de login por minuto
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de seguridad (orden importante)
// 1. HTTPS y CORS primero
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 2. Rate limiting
app.UseIpRateLimiting();

// 3. Autenticación y autorización (antes de los middleware personalizados)
app.UseAuthentication();
app.UseAuthorization();

// 4. Middleware personalizados (después de autenticación)
app.UseMiddleware<SensitiveDataLoggingMiddleware>(); // Filtrado de datos sensibles
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // Manejo seguro de errores

// 5. Mapeo de controladores
app.MapControllers();

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        
        if (pendingMigrations.Any())
        {
            app.Logger.LogInformation($"Aplicando {pendingMigrations.Count} migración(es) pendiente(s): {string.Join(", ", pendingMigrations)}");
            db.Database.Migrate();
            app.Logger.LogInformation("Migraciones de EventService aplicadas correctamente");
        }
        else
        {
            app.Logger.LogInformation("No hay migraciones pendientes. Base de datos actualizada.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al aplicar migraciones de EventService");
        throw;
    }
}

app.Run();
