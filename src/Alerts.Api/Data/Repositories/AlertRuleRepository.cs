using Alerts.Api.Data.Models;

namespace Alerts.Api.Data.Repositories;

public class AlertRuleRepository
{
    private readonly AppDbContext _context;

    public AlertRuleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AlertRule rule)
    {
        await _context.AlertRules.AddAsync(rule);
        await _context.SaveChangesAsync();
    }
}
