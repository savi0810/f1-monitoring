namespace Telemetry.Api.Data.Repositories;

public class SessionDriverRepository
{
    private readonly AppDbContext _context;

    public SessionDriverRepository(AppDbContext context)
    {
        _context = context;
    }
}
