using System.Text.Json;
using F1Monitor.Contracts;
using StackExchange.Redis;

namespace Live.Api.Data.Repositories;

public class LiveSnapshotRepository : ILiveSnapshotRepository
{
    private const string CurrentKey = "live:snapshot:current";
    private readonly IDatabase _db;

    public LiveSnapshotRepository(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<LiveRaceSnapshot?> GetAsync(string? sessionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = sessionId == null ? CurrentKey : $"live:snapshot:session:{sessionId}";
        var json = await _db.StringGetAsync(key);

        return json.IsNull ? null : JsonSerializer.Deserialize<LiveRaceSnapshot>(json.ToString());
    }

    public async Task<(bool Saved, LiveRaceSnapshot? Previous)> SaveIfNewerAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var key = $"live:snapshot:session:{snapshot.SessionId}";
        var json = JsonSerializer.Serialize(snapshot);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var previousJson = await _db.StringGetAsync(key);
            var previous = previousJson.IsNull
                ? null
                : JsonSerializer.Deserialize<LiveRaceSnapshot>(previousJson.ToString())
                    ?? throw new JsonException("Stored snapshot is empty.");

            if (previous != null && snapshot.ObservedAt <= previous.ObservedAt)
                return (false, previous);

            var currentJson = await _db.StringGetAsync(CurrentKey);
            var current = currentJson.IsNull
                ? null
                : JsonSerializer.Deserialize<LiveRaceSnapshot>(currentJson.ToString())
                    ?? throw new JsonException("Current snapshot is empty.");

            var transaction = _db.CreateTransaction();
            transaction.AddCondition(previousJson.IsNull
                ? Condition.KeyNotExists(key)
                : Condition.StringEqual(key, previousJson));
            transaction.AddCondition(currentJson.IsNull
                ? Condition.KeyNotExists(CurrentKey)
                : Condition.StringEqual(CurrentKey, currentJson));

            _ = transaction.StringSetAsync(key, json);

            if (current == null || snapshot.ObservedAt > current.ObservedAt)
                _ = transaction.StringSetAsync(CurrentKey, json);

            if (await transaction.ExecuteAsync())
                return (true, previous);
        }
    }
}
