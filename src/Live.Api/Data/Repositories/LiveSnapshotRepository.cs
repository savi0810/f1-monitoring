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

    public async Task<(bool Saved, LiveRaceSnapshot? Previous)> SaveIfNewerAsync(LiveRaceSnapshot snapshot, Func<LiveRaceSnapshot?, PendingLiveEventDto?> createEvent, CancellationToken cancellationToken = default)
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

            var pendingEvent = createEvent(previous);
            var pendingEventJson = pendingEvent == null
                ? null
                : JsonSerializer.Serialize(pendingEvent);
            var outboxLength = pendingEventJson != null
                ? await _db.ListLengthAsync(OutboxKey)
                : 0;

            var transaction = _db.CreateTransaction();
            transaction.AddCondition(previousJson.IsNull
                ? Condition.KeyNotExists(key)
                : Condition.StringEqual(key, previousJson));
            transaction.AddCondition(currentJson.IsNull
                ? Condition.KeyNotExists(CurrentKey)
                : Condition.StringEqual(CurrentKey, currentJson));

            if (pendingEventJson != null)
                transaction.AddCondition(Condition.ListLengthEqual(OutboxKey, outboxLength));

            _ = transaction.StringSetAsync(key, json);

            if (current == null || snapshot.ObservedAt > current.ObservedAt)
                _ = transaction.StringSetAsync(CurrentKey, json);

            Task<long>? enqueueTask = null;
            if (pendingEventJson != null)
                enqueueTask = transaction.ListRightPushAsync(OutboxKey, pendingEventJson);

            if (await transaction.ExecuteAsync())
            {
                if (enqueueTask != null)
                    await enqueueTask;

                return (true, previous);
            }
        }
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
