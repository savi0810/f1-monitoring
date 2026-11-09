using F1Monitor.Contracts;

namespace Live.Api.Data.Repositories;

public interface ILiveSnapshotRepository
{
    Task<LiveRaceSnapshot?> GetAsync(string? sessionId, CancellationToken cancellationToken = default);

    Task<(bool Saved, LiveRaceSnapshot? Previous)> SaveIfNewerAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken = default);
}
