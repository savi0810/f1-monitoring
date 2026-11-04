using F1Monitor.Contracts;
using Live.Api.Data.Models;
using Live.Api.Data.Repositories;

namespace Live.Api.Services;

public class TimingService
{
    private readonly ILiveSnapshotRepository _repository;

    public TimingService(ILiveSnapshotRepository repository)
    {
        _repository = repository;
    }

    public async Task ProcessAsync(RaceEvent raceEvent, CancellationToken cancellationToken = default)
    {
        var payload = EventSerializer.ReadPayload<TimingUpdatedPayload>(raceEvent);
        var current = await _repository.GetAsync(raceEvent.SessionKey, cancellationToken);

        if (current is not null && payload.Version <= current.Version)
            return;

        var snapshot = new LiveSnapshot
        {
            SessionKey = raceEvent.SessionKey,
            Version = payload.Version,
            UpdatedAt = raceEvent.OccurredAt ?? DateTimeOffset.UtcNow,
            Drivers = payload.Drivers.Select(driver => new LiveDriver
            {
                DriverNumber = driver.DriverNumber,
                Position = driver.Position,
                LapNumber = driver.LapNumber,
                LastLapSeconds = driver.LastLapSeconds,
                GapToLeader = driver.GapToLeader
            }).ToList()
        };

        await _repository.SaveAsync(snapshot, cancellationToken);
    }
}
