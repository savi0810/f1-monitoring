using System.Text.Json;
using Live.Api.Data.Models;
using StackExchange.Redis;

namespace Live.Api.Data.Repositories;

public class LiveSourceStateRepository : ILiveSourceStateRepository
{
    private readonly IDatabase _db;

    public LiveSourceStateRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<LiveSourceState?> GetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var json = await _db.StringGetAsync("live:source:state");
        return json.IsNull ? null : JsonSerializer.Deserialize<LiveSourceState>(json.ToString());
    }

    public Task SaveAsync(LiveSourceState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var json = JsonSerializer.Serialize(state);
        return _db.StringSetAsync("live:source:state", json);
    }
}
