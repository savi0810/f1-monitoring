using Alerts.Api.Data.Models;

namespace Alerts.Api.Data.Repositories;

public interface IDriverSubscriptionRepository
{
    Task<IReadOnlyList<DriverSubscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DriverSubscription> AddOrGetAsync(DriverSubscription subscription, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, string driverId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DriverSubscription>> GetSubscribersAsync(IReadOnlyCollection<string> driverIds, DateTimeOffset observedAt, CancellationToken cancellationToken = default);
}
