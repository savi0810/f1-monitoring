using Alerts.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<AlertRule>> GetByUserIdAsync(Guid userId)
    {
        return await _context.AlertRules
            .AsNoTracking()
            .Where(rule => rule.UserId == userId)
            .ToListAsync();
    }
}
