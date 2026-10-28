using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<RaceEventRecord> RaceEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RaceEventRecord>()
            .HasKey(e => e.EventId);

        modelBuilder.Entity<RaceEventRecord>()
            .HasIndex(e => e.SessionKey);
    }
}
