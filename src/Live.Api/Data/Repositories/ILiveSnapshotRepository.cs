using F1Monitor.Contracts;
using Live.Api.Data.DTOs;

namespace Live.Api.Data.Repositories;

public interface ILiveSnapshotRepository
{
    Task<LiveRaceSnapshot?> GetAsync(string? sessionId, CancellationToken cancellationToken = default);

    Task<(bool Saved, LiveRaceSnapshot? Previous)> SaveIfNewerAsync(LiveRaceSnapshot snapshot, Func<LiveRaceSnapshot?, PendingLiveEventDto?> createEvent, CancellationToken cancellationToken = default);

    Task<PendingLiveEventDto?> GetPendingEventAsync(CancellationToken cancellationToken = default);

    Task<bool> RemovePendingEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
