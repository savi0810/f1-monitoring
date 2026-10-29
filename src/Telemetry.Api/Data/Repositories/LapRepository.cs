using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public class LapRepository : ILapRepository
{
    private readonly AppDbContext _context;

    public LapRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddLapAsync(Lap lap)
    {
        var inserted = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Laps" ("EventId", "SessionKey", "DriverNumber", "LapNumber", "LapDuration",
                                "Sector1Duration", "Sector2Duration", "Sector3Duration", "IsPitOutLap", "OccurredAt")
            VALUES ({lap.EventId}, {lap.SessionKey}, {lap.DriverNumber}, {lap.LapNumber}, {lap.LapDuration},
                    {lap.Sector1Duration}, {lap.Sector2Duration}, {lap.Sector3Duration}, {lap.IsPitOutLap}, {lap.OccurredAt?.ToUniversalTime()})
            ON CONFLICT ("EventId") DO NOTHING
            """);

        return inserted > 0;
    }

    public async Task<IReadOnlyList<SessionSummary>> GetSessionsAsync()
    {
        var sessionTimestamps = await _context.Laps
            .Select(l => new { l.SessionKey, l.OccurredAt })
            .ToListAsync();

        return sessionTimestamps
            .GroupBy(l => l.SessionKey)
            .Select(g => new SessionSummary(
                g.Key,
                g.Count(),
                g.Min(l => l.OccurredAt),
                g.Max(l => l.OccurredAt)))
            .OrderByDescending(r => r.EndedAt)
            .ToList();
    }

    public async Task<IReadOnlyList<Lap>> GetLapsBySessionKeyAsync(int sessionKey)
    {
        return await _context.Laps
            .Where(l => l.SessionKey == sessionKey)
            .OrderBy(l => l.DriverNumber)
            .ThenBy(l => l.LapNumber)
            .ToListAsync();
    }
}
