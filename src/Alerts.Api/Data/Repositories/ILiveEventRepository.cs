using Alerts.Api.Data.Models;

namespace Alerts.Api.Data.Repositories;

public interface ILiveEventRepository
{
    Task SaveAsync(Guid eventId, IReadOnlyCollection<OutboxMessage> messages, CancellationToken cancellationToken = default);

    Task<bool> SaveCompletionAsync(CompletedRace race, CancellationToken cancellationToken = default);
}
