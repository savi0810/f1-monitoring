using F1Monitor.Contracts;
using Live.Api.Data.DTOs;
using Live.Api.Data.Models;
using Live.Api.Data.Repositories;

namespace Live.Api.Services;

public class LiveSourceService
{
    private readonly ILiveSourceStateRepository _repository;
    private readonly TimingService _timingService;

    public LiveSourceService(ILiveSourceStateRepository repository, TimingService timingService)
    {
        _repository = repository;
        _timingService = timingService;
    }

    public async Task<CurrentLiveDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var state = await _repository.GetAsync(cancellationToken);
        if (state is null)
            return new CurrentLiveDto(false, null);

        var isConnected = state.IsConnected && state.ReportedAt >= DateTimeOffset.UtcNow.AddSeconds(-30);
        var snapshot = state.SessionKey is int sessionKey
            ? await _timingService.GetSnapshotAsync(sessionKey, cancellationToken)
            : null;

        return new CurrentLiveDto(isConnected, snapshot);
    }

    public async Task ProcessAsync(LiveSourceStatus status, CancellationToken cancellationToken = default)
    {
        var current = await _repository.GetAsync(cancellationToken);
        if (current is not null && status.ReportedAt <= current.ReportedAt)
            return;

        var state = new LiveSourceState
        {
            IsConnected = status.IsConnected,
            SessionKey = status.SessionKey,
            ReportedAt = status.ReportedAt
        };

        await _repository.SaveAsync(state, cancellationToken);
    }
}
