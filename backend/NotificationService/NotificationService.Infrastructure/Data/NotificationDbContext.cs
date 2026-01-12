using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    public DbSet<NotificationLog> NotificationLogs { get; set; }
    public DbSet<ProcessedMessage> ProcessedMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar schema
        modelBuilder.HasDefaultSchema("notifications");

        // Configurar NotificationLog
        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.EventId).IsRequired();
            entity.Property(n => n.EventName).IsRequired().HasMaxLength(200);
            entity.Property(n => n.OccurredAt).IsRequired();
            entity.Property(n => n.CorrelationId).IsRequired();
            entity.Property(n => n.PayloadHash).IsRequired().HasMaxLength(64);
            entity.Property(n => n.Status).IsRequired().HasMaxLength(50);
            entity.Property(n => n.CreatedAt).IsRequired();
            
            // Índice para búsquedas por EventId
            entity.HasIndex(n => n.EventId);
            entity.HasIndex(n => n.CorrelationId);
        });

        // Configurar ProcessedMessage (para idempotencia)
        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.MessageId).IsRequired();
            entity.Property(p => p.ProcessedAt).IsRequired();
            entity.Property(p => p.Status).IsRequired().HasMaxLength(50);
            
            // Índice único para MessageId (idempotencia)
            entity.HasIndex(p => p.MessageId).IsUnique();
        });
    }
}
