namespace Alerts.Api.Data.Repositories;

public class DriverSubscriptionRepository
{
    private readonly AppDbContext _context;

    public DriverSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }
}
