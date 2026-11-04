using System.Text.Json;
using Live.Api.Data.Models;
using StackExchange.Redis;

namespace Live.Api.Data.Repositories;

public class LiveSnapshotRepository : ILiveSnapshotRepository
{
    private readonly IDatabase _db;

    public LiveSnapshotRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<LiveSnapshot?> GetAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var json = await _db.StringGetAsync($"live:snapshot:json:{sessionKey}");
        return json.IsNull ? null : JsonSerializer.Deserialize<LiveSnapshot>(json.ToString());
    }

    public Task SaveAsync(LiveSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var json = JsonSerializer.Serialize(snapshot);
        return _db.StringSetAsync($"live:snapshot:json:{snapshot.SessionKey}", json);
    }

    public async Task<LiveSnapshot> SaveIfNewerAsync(LiveSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var key = $"live:snapshot:json:{snapshot.SessionKey}";
        var json = JsonSerializer.Serialize(snapshot);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentJson = await _db.StringGetAsync(key);
            if (!currentJson.IsNull)
            {
                var current = JsonSerializer.Deserialize<LiveSnapshot>(currentJson.ToString())
                    ?? throw new JsonException("Stored snapshot is empty.");

                if (snapshot.Version <= current.Version)
                    return current;
            }

            var transaction = _db.CreateTransaction();
            transaction.AddCondition(currentJson.IsNull
                ? Condition.KeyNotExists(key)
                : Condition.StringEqual(key, currentJson));
            _ = transaction.StringSetAsync(key, json);

            if (await transaction.ExecuteAsync())
                return snapshot;
        }
    }
}
