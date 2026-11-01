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

    public DbSet<Session> Sessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Lap>()
            .HasKey(l => l.EventId);

        modelBuilder.Entity<Lap>()
            .HasIndex(l => l.SessionKey);

        modelBuilder.Entity<Session>()
            .HasKey(s => s.SessionKey);

        modelBuilder.Entity<Session>()
            .Property(s => s.SessionKey)
            .ValueGeneratedNever();

        modelBuilder.Entity<Session>()
            .HasIndex(s => new { s.Year, s.SessionType });

        modelBuilder.Entity<Session>()
            .HasIndex(s => s.DateStart);
    }
}
