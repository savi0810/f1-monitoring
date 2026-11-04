using System.Text.Json;
using Live.Api.Data.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Live.Api.Data.Repositories;

public class LiveSnapshotRepository : ILiveSnapshotRepository
{
    private readonly IDistributedCache _cache;

    public LiveSnapshotRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<LiveSnapshot?> GetAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        var json = await _cache.GetStringAsync($"live:snapshot:{sessionKey}", cancellationToken);
        return json is null ? null : JsonSerializer.Deserialize<LiveSnapshot>(json);
    }

    public Task SaveAsync(LiveSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(snapshot);
        return _cache.SetStringAsync($"live:snapshot:{snapshot.SessionKey}", json, cancellationToken);
    }
}
