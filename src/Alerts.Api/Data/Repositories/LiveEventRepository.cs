namespace Alerts.Api.Data.Repositories;

public class LiveEventRepository
{
    private readonly AppDbContext _context;

    public LiveEventRepository(AppDbContext context)
    {
        _context = context;
    }
}
