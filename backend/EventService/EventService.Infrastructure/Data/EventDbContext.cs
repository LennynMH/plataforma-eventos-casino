using Microsoft.EntityFrameworkCore;
using EventService.Domain.Entities;

namespace EventService.Infrastructure.Data;

public class EventDbContext : DbContext
{
    public EventDbContext(DbContextOptions<EventDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Zone> Zones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar schema
        modelBuilder.HasDefaultSchema("events");

        // Configurar Event
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Location).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasMany(e => e.Zones)
                .WithOne(z => z.Event)
                .HasForeignKey(z => z.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurar Zone
        modelBuilder.Entity<Zone>(entity =>
        {
            entity.HasKey(z => z.Id);
            entity.Property(z => z.EventId).IsRequired();
            entity.Property(z => z.Name).IsRequired().HasMaxLength(250);
            entity.Property(z => z.Price).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(z => z.Capacity).IsRequired();
            entity.Property(z => z.CreatedAt).IsRequired();
        });
    }
}
