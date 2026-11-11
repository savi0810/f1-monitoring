using Alerts.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Data.Repositories;

public class DriverSubscriptionRepository : IDriverSubscriptionRepository
{
    private readonly AppDbContext _context;

    public DriverSubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DriverSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.DriverSubscriptions
            .AsNoTracking()
            .Where(subscription => subscription.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DriverSubscription> AddOrGetAsync(DriverSubscription subscription, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _context.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "DriverSubscriptions" ("Id", "UserId", "DriverId", "CreatedAt")
                VALUES ({subscription.Id}, {subscription.UserId}, {subscription.DriverId}, {subscription.CreatedAt})
                ON CONFLICT ("UserId", "DriverId") DO NOTHING
                """, cancellationToken);

            var stored = await _context.DriverSubscriptions
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item => item.UserId == subscription.UserId
                        && item.DriverId == subscription.DriverId,
                    cancellationToken);

            if (stored != null)
                return stored;
        }
    }

    public async Task DeleteAsync(Guid userId, string driverId, CancellationToken cancellationToken = default)
    {
        await _context.DriverSubscriptions
            .Where(subscription => subscription.UserId == userId
                && subscription.DriverId == driverId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DriverSubscription>> GetSubscribersAsync(IReadOnlyCollection<string> driverIds, DateTimeOffset observedAt, CancellationToken cancellationToken = default)
    {
        return await _context.DriverSubscriptions
            .AsNoTracking()
            .Where(subscription => driverIds.Contains(subscription.DriverId)
                && subscription.CreatedAt <= observedAt)
            .ToListAsync(cancellationToken);
    }
}
