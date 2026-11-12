using Alerts.Api.Data.DTOs;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;

namespace Alerts.Api.Services;

public class DriverSubscriptionService : IDriverSubscriptionService
{
    private readonly IDriverSubscriptionRepository _repository;
    private readonly ILogger<DriverSubscriptionService> _logger;

    public DriverSubscriptionService(IDriverSubscriptionRepository repository, ILogger<DriverSubscriptionService> logger)
    {
        _repository = repository;
        _logger = logger;
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
        _logger.LogInformation("Subscription {SubscriptionId} is active for user {UserId} and driver {DriverId}", stored.Id, userId, stored.DriverId);

        return new DriverSubscriptionDto(stored.Id, stored.DriverId, stored.CreatedAt);
    }

    public async Task DeleteAsync(Guid userId, string driverId, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(userId, driverId, cancellationToken);
        _logger.LogInformation("Subscription removal completed for user {UserId} and driver {DriverId}", userId, driverId);
    }
}
