namespace Alerts.Api.Data.Repositories;

public class AlertRuleRepository
{
    private readonly AppDbContext _context;

    public AlertRuleRepository(AppDbContext context)
    {
        _context = context;
    }
}
