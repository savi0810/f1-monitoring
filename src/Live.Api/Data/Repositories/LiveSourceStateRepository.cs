using System.Text.Json;
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

    public async Task<LiveSourceState?> GetAsync(CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync("live:source", cancellationToken);
        return json is null ? null : JsonSerializer.Deserialize<LiveSourceState>(json);
    }

    public Task SaveAsync(LiveSourceState state, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(state);
        return _cache.SetStringAsync("live:source", json, cancellationToken);
    }
}
