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
    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProcessedEvent>()
            .HasKey(processedEvent => processedEvent.EventId);
    }
}
