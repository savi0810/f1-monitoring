using System.Text.Json;
using F1Monitor.Contracts;
using Live.Api.Data.DTOs;
using StackExchange.Redis;

namespace Live.Api.Data.Repositories;

public class LiveSnapshotRepository : ILiveSnapshotRepository
{
    private const string CurrentKey = "live:snapshot:current";
    private const string OutboxKey = "live:alerts:outbox";
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

    public async Task<bool> TrySaveAsync(LiveRaceSnapshot snapshot, Guid? expectedEventId, PendingLiveEventDto? pendingEvent, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var key = $"live:snapshot:session:{snapshot.SessionId}";
        var previousJson = await _db.StringGetAsync(key);
        var previous = DeserializeSnapshot(previousJson);

        if (previous?.EventId != expectedEventId)
            return false;

        var currentJson = await _db.StringGetAsync(CurrentKey);
        var transaction = _db.CreateTransaction();

        AddSnapshotCondition(transaction, key, previousJson);
        AddSnapshotCondition(transaction, CurrentKey, currentJson);

        var writes = QueueSnapshotWrites(transaction, key, snapshot, currentJson, pendingEvent);

        if (!await transaction.ExecuteAsync())
            return false;

        await Task.WhenAll(writes);
        return true;
    }

    private static LiveRaceSnapshot? DeserializeSnapshot(RedisValue json)
    {
        return json.IsNull
            ? null
            : JsonSerializer.Deserialize<LiveRaceSnapshot>(json.ToString())
                ?? throw new JsonException("Stored snapshot is empty.");
    }

    private static void AddSnapshotCondition(ITransaction transaction, string key, RedisValue expectedJson)
    {
        transaction.AddCondition(expectedJson.IsNull
            ? Condition.KeyNotExists(key)
            : Condition.StringEqual(key, expectedJson));
    }

    private static List<Task> QueueSnapshotWrites(ITransaction transaction, string key, LiveRaceSnapshot snapshot, RedisValue currentJson, PendingLiveEventDto? pendingEvent)
    {
        var current = DeserializeSnapshot(currentJson);
        var json = JsonSerializer.Serialize(snapshot);
        var writes = new List<Task>
        {
            transaction.StringSetAsync(key, json)
        };

        if (current == null || snapshot.ObservedAt > current.ObservedAt)
            writes.Add(transaction.StringSetAsync(CurrentKey, json));

        if (pendingEvent != null)
            writes.Add(transaction.ListRightPushAsync(
                OutboxKey, JsonSerializer.Serialize(pendingEvent)));

        return writes;
    }

    public async Task<PendingLiveEventDto?> GetPendingEventAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var json = await _db.ListGetByIndexAsync(OutboxKey, 0);
        if (json.IsNull)
            return null;

        return JsonSerializer.Deserialize<PendingLiveEventDto>(json.ToString())
            ?? throw new JsonException("Stored pending event is empty.");
    }

    public async Task<bool> RemovePendingEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var json = await _db.ListGetByIndexAsync(OutboxKey, 0);
        if (json.IsNull)
            return false;

        var pendingEvent = JsonSerializer.Deserialize<PendingLiveEventDto>(json.ToString())
            ?? throw new JsonException("Stored pending event is empty.");

        if (pendingEvent.EventId != eventId)
            return false;

        var transaction = _db.CreateTransaction();
        transaction.AddCondition(Condition.ListIndexEqual(OutboxKey, 0, json));

        var removeTask = transaction.ListLeftPopAsync(OutboxKey);
        if (!await transaction.ExecuteAsync())
            return false;

        await removeTask;
        return true;
    }
}
