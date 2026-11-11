using F1Monitor.Contracts;
using Live.Api.Data.DTOs;
using Live.Api.Data.Repositories;
using Live.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Live.Api.Services;

public class TimingService
{
    private readonly ILiveSnapshotRepository _repository;
    private readonly IHubContext<LiveHub> _hub;
    private readonly PositionChangeDetector _positionChangeDetector;
    private readonly RaceCompletionDetector _raceCompletionDetector;

    public TimingService(ILiveSnapshotRepository repository, IHubContext<LiveHub> hub, PositionChangeDetector positionChangeDetector, RaceCompletionDetector raceCompletionDetector)
    {
        _repository = repository;
        _hub = hub;
        _positionChangeDetector = positionChangeDetector;
        _raceCompletionDetector = raceCompletionDetector;
    }

    public Task<LiveRaceSnapshot?> GetSnapshotAsync(string? sessionId, CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(sessionId, cancellationToken);
    }

    public async Task ProcessAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var result = await SaveSnapshotAsync(snapshot, cancellationToken);
        if (!result.Saved)
            snapshot = result.Previous
                ?? throw new InvalidOperationException("Current snapshot is missing.");

        var group = _hub.Clients.Group($"session:{snapshot.SessionId}");

        await group.SendAsync("SourceUpdated", new
        {
            snapshot.SessionId,
            snapshot.ObservedAt
        }, cancellationToken);

        var previous = result.Previous;
        if (result.Saved && previous != null)
        {
            var comparable = snapshot with
            {
                EventId = previous.EventId,
                ObservedAt = previous.ObservedAt,
                Drivers = previous.Drivers
            };

            if (comparable == previous && snapshot.Drivers.SequenceEqual(previous.Drivers))
                return;
        }

        await group.SendAsync("TimingUpdated", snapshot, cancellationToken);
    }

    private async Task<(bool Saved, LiveRaceSnapshot? Previous)> SaveSnapshotAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var previous = await _repository.GetAsync(snapshot.SessionId, cancellationToken);
            if (previous != null && snapshot.ObservedAt <= previous.ObservedAt)
                return (false, previous);

            var recordedCompletion = await _repository.GetCompletionAsync(snapshot.SessionId, cancellationToken);
            var completion = recordedCompletion == null
                ? _raceCompletionDetector.Detect(snapshot)
                : null;
            var pendingEvent = recordedCompletion == null
                ? CreateEvent(previous, snapshot, completion)
                : null;

            if (await _repository.TrySaveAsync(snapshot, previous?.EventId, pendingEvent, completion, cancellationToken))
                return (true, previous);
        }
    }

    private PendingLiveEventDto? CreateEvent(LiveRaceSnapshot? previous, LiveRaceSnapshot snapshot, RaceCompleted? completion)
    {
        if (completion != null)
        {
            return new PendingLiveEventDto(
                completion.EventId,
                RoutingKeys.For(RoutingKeys.Live, EventTypes.RaceCompleted),
                EventSerializer.Serialize(completion));
        }

        var changes = _positionChangeDetector.Detect(previous, snapshot);
        if (changes.Count == 0)
            return null;

        var message = new DriverPositionsChanged(
            snapshot.EventId,
            snapshot.SessionId,
            snapshot.ObservedAt,
            changes);

        return new PendingLiveEventDto(
            message.EventId,
            RoutingKeys.For(RoutingKeys.Live, EventTypes.DriverPositionsChanged),
            EventSerializer.Serialize(message));
    }
}
