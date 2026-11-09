using F1Monitor.Contracts;
using Live.Api.Data.Repositories;
using Live.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Live.Api.Services;

public class TimingService
{
    private readonly ILiveSnapshotRepository _repository;
    private readonly IHubContext<LiveHub> _hub;

    public TimingService(ILiveSnapshotRepository repository, IHubContext<LiveHub> hub)
    {
        _repository = repository;
        _hub = hub;
    }

    public Task<LiveRaceSnapshot?> GetSnapshotAsync(string? sessionId, CancellationToken cancellationToken = default)
    {
        return _repository.GetAsync(sessionId, cancellationToken);
    }

    public async Task ProcessAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var result = await _repository.SaveIfNewerAsync(snapshot, cancellationToken);
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
}
