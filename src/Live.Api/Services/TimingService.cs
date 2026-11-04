using F1Monitor.Contracts;
using Live.Api.Data.DTOs;
using Live.Api.Data.Models;
using Live.Api.Data.Repositories;
using Live.Api.Hubs;
using Live.Api.Mapping;
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

    public async Task<LiveSnapshotDto?> GetSnapshotAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        var snapshot = await _repository.GetAsync(sessionKey, cancellationToken);
        return snapshot is null ? null : LiveSnapshotMapper.ToDto(snapshot);
    }

    public async Task ProcessAsync(RaceEvent raceEvent, CancellationToken cancellationToken = default)
    {
        var payload = EventSerializer.ReadPayload<TimingUpdatedPayload>(raceEvent);
        var candidate = new LiveSnapshot
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

        var snapshot = await _repository.SaveIfNewerAsync(candidate, cancellationToken);

        if (payload.Version < snapshot.Version)
            return;

        await _hub.Clients.Group($"session:{snapshot.SessionKey}")
            .SendAsync("TimingUpdated", LiveSnapshotMapper.ToDto(snapshot), cancellationToken);
    }
}
