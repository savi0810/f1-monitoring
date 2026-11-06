namespace Telemetry.Api.Data.Repositories;

public class MeetingRepository
{
    private readonly AppDbContext _context;

    public MeetingRepository(AppDbContext context)
    {
        _context = context;
    }
}
