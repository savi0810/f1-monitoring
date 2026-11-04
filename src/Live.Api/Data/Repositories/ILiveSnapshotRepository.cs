using Live.Api.Data.Models;

namespace Live.Api.Data.Repositories;

public interface ILiveSnapshotRepository
{
    Task<LiveSnapshot?> GetAsync(int sessionKey, CancellationToken cancellationToken = default);

    Task SaveAsync(LiveSnapshot snapshot, CancellationToken cancellationToken = default);

    Task<LiveSnapshot> SaveIfNewerAsync(LiveSnapshot snapshot, CancellationToken cancellationToken = default);
}
