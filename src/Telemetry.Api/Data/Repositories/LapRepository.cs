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
        await _context.Laps.AddAsync(lap);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            _context.Entry(lap).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<IReadOnlyList<SessionSummary>> GetRacesAsync()
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
