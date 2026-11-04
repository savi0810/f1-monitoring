using Live.Api.Data.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Live.Api.Data.Repositories;

public class LiveSourceStateRepository : ILiveSourceStateRepository
{
    private readonly IDistributedCache _cache;

    public LiveSourceStateRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public Task<LiveSourceState?> GetAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SaveAsync(LiveSourceState state, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
