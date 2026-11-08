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
        var isConnected = snapshot != null
            && snapshot.ObservedAt >= DateTimeOffset.UtcNow.AddSeconds(-30);

        return new CurrentLiveDto(isConnected, snapshot);
    }
}
