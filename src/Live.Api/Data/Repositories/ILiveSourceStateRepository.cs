using Live.Api.Data.Models;

namespace Live.Api.Data.Repositories;

public interface ILiveSourceStateRepository
{
    Task<LiveSourceState?> GetAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(LiveSourceState state, CancellationToken cancellationToken = default);
}
