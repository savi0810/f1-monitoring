using F1Monitor.Contracts;

namespace Alerts.Api.Services;

public interface IRaceCompletionService
{
    Task ProcessAsync(RaceCompleted message, CancellationToken cancellationToken = default);
}
