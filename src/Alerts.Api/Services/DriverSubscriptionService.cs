using Alerts.Api.Data.DTOs;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;

namespace Alerts.Api.Services;

public class DriverSubscriptionService : IDriverSubscriptionService
{
    private readonly IDriverSubscriptionRepository _repository;

    public DriverSubscriptionService(IDriverSubscriptionRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DriverSubscriptionDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var subscriptions = await _repository.GetByUserIdAsync(userId, cancellationToken);

        return subscriptions.Select(subscription =>
            new DriverSubscriptionDto(subscription.Id, subscription.DriverId, subscription.CreatedAt)).ToList();
    }

    public async Task<DriverSubscriptionDto> CreateAsync(Guid userId, CreateDriverSubscriptionDto request, CancellationToken cancellationToken = default)
    {
        var subscription = new DriverSubscription
        {
            UserId = userId,
            DriverId = request.DriverId
        };

        var stored = await _repository.AddOrGetAsync(subscription, cancellationToken);

        return new DriverSubscriptionDto(stored.Id, stored.DriverId, stored.CreatedAt);
    }

    public Task DeleteAsync(Guid userId, string driverId, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(userId, driverId, cancellationToken);
    }
}
