using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Lap> Laps { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lap>()
            .HasKey(l => l.EventId);

        modelBuilder.Entity<Lap>()
            .HasIndex(l => l.SessionKey);
    }
}
