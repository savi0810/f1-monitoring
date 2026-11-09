using Live.Api.Data.DTOs;

namespace Live.Api.Services;

public class LiveSourceService
{
    private readonly TimingService _timingService;

    public LiveSourceService(TimingService timingService)
    {
        _timingService = timingService;
    }

    public async Task<CurrentLiveDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _timingService.GetSnapshotAsync(null, cancellationToken);
        var maxAgeSeconds = Math.Max(30, (snapshot?.ExpectedUpdateInterval?.TotalSeconds ?? 0) + 30);
        var isConnected = snapshot != null
            && (DateTimeOffset.UtcNow - snapshot.ObservedAt).TotalSeconds <= maxAgeSeconds;

        return new CurrentLiveDto(isConnected, snapshot);
    }
}
