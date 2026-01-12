using Microsoft.EntityFrameworkCore;
using MassTransit;
using NotificationService.Infrastructure.Data;
using NotificationService.Infrastructure.Email;
using NotificationService.Infrastructure.Consumers;
using EventService.Domain.Events;
using EventService.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Notification Service API", 
        Version = "v1" 
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("NotificationDb");
builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Configurar el esquema para la tabla de historial de migraciones
        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notifications");
        // Especificar el assembly donde están las migraciones
        npgsqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.GetName().Name);
    });
});

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// MassTransit (RabbitMQ) - Consumer
builder.Services.AddMassTransit(x =>
{
    // Registrar consumer que acepta el tipo EventCreated local
    x.AddConsumer<EventCreatedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "admin123");
        });

        // Configurar nombre de contrato personalizado para EventCreated
        // Debe coincidir con el nombre usado en EventService
        cfg.Message<EventCreated>(e =>
        {
            e.SetEntityName("EventCreated");
        });

        // Configurar consumer con reintentos y DLQ
        cfg.ReceiveEndpoint("event-created-queue", e =>
        {
            // Bind al exchange "EventCreated" para recibir mensajes publicados
            // El exchange se crea con el nombre del contrato configurado arriba
            e.Bind("EventCreated");
            
            // Configurar el consumer que acepta EventCreated
            e.ConfigureConsumer<EventCreatedConsumer>(context);
            
            // Configurar reintentos
            e.UseMessageRetry(r => r.Exponential(
                retryLimit: 3,
                minInterval: TimeSpan.FromSeconds(1),
                maxInterval: TimeSpan.FromSeconds(10),
                intervalDelta: TimeSpan.FromSeconds(2)));

            // Configurar DLQ (Dead Letter Queue) explícitamente con nombre personalizado
            e.BindDeadLetterQueue("event-created-dlq");
        });
    });
});

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

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "NotificationService" }));

// Aplicar migraciones automáticamente al iniciar
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var pendingMigrations = db.Database.GetPendingMigrations().ToList();
        
        if (pendingMigrations.Any())
        {
            app.Logger.LogInformation($"Aplicando {pendingMigrations.Count} migración(es) pendiente(s): {string.Join(", ", pendingMigrations)}");
            db.Database.Migrate();
            app.Logger.LogInformation("Migraciones de NotificationService aplicadas correctamente");
        }
        else
        {
            app.Logger.LogInformation("No hay migraciones pendientes. Base de datos actualizada.");
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error al aplicar migraciones de NotificationService");
        throw;
    }
}

app.Run();
