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
}
