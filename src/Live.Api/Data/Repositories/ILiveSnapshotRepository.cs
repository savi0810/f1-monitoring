using F1Monitor.Contracts;
using Live.Api.Data.DTOs;

namespace Live.Api.Data.Repositories;

public interface ILiveSnapshotRepository
{
    Task<LiveRaceSnapshot?> GetAsync(string? sessionId, CancellationToken cancellationToken = default);

    Task<RaceCompleted?> GetCompletionAsync(string sessionId, CancellationToken cancellationToken = default);

    Task<bool> TrySaveAsync(LiveRaceSnapshot snapshot, Guid? expectedEventId, PendingLiveEventDto? pendingEvent, RaceCompleted? completion, CancellationToken cancellationToken = default);

    Task<PendingLiveEventDto?> GetPendingEventAsync(CancellationToken cancellationToken = default);

    Task<bool> RemovePendingEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
