using Alerts.Api.Data.DTOs;

namespace Alerts.Api.Services;

public interface IDriverSubscriptionService
{
    Task<IReadOnlyList<DriverSubscriptionDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<DriverSubscriptionDto> CreateAsync(Guid userId, CreateDriverSubscriptionDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, string driverId, CancellationToken cancellationToken = default);
}
