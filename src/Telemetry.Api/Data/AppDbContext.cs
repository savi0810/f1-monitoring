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

    public DbSet<Meeting> Meetings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>()
            .HasKey(m => m.MeetingKey);

        modelBuilder.Entity<Meeting>()
            .Property(m => m.MeetingKey)
            .ValueGeneratedNever();

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
            .HasOne<Meeting>()
            .WithMany()
            .HasForeignKey(s => s.MeetingKey)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Session>()
            .HasIndex(s => new { s.Year, s.SessionType });

        modelBuilder.Entity<Session>()
            .HasIndex(s => s.DateStart);
    }
}
