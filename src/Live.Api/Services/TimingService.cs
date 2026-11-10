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

    public TimingService(ILiveSnapshotRepository repository, IHubContext<LiveHub> hub, PositionChangeDetector positionChangeDetector)
    {
        _repository = repository;
        _hub = hub;
        _positionChangeDetector = positionChangeDetector;
    }

    public Task<LiveRaceSnapshot?> GetSnapshotAsync(string? sessionId, CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(sessionId, cancellationToken);
    }

    public async Task ProcessAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SaveIfNewerAsync(snapshot, previous => CreateEvent(previous, snapshot), cancellationToken);
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

    private PendingLiveEventDto? CreateEvent(LiveRaceSnapshot? previous, LiveRaceSnapshot snapshot)
    {
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
