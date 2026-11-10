using Alerts.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<DriverSubscription> DriverSubscriptions { get; set; }
    public DbSet<CompletedRace> CompletedRaces { get; set; }
    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DriverSubscription>()
            .Property(subscription => subscription.DriverId)
            .HasMaxLength(128)
            .IsRequired();

        modelBuilder.Entity<DriverSubscription>()
            .HasIndex(subscription => new { subscription.UserId, subscription.DriverId })
            .IsUnique();

        modelBuilder.Entity<DriverSubscription>()
            .HasIndex(subscription => subscription.DriverId);

        modelBuilder.Entity<CompletedRace>()
            .HasKey(race => race.SessionId);

        modelBuilder.Entity<CompletedRace>()
            .Property(race => race.SessionId)
            .HasMaxLength(128)
            .IsRequired();

        modelBuilder.Entity<CompletedRace>()
            .HasIndex(race => race.RecordedAt);

        modelBuilder.Entity<ProcessedEvent>()
            .HasKey(processedEvent => processedEvent.EventId);
    }
}
