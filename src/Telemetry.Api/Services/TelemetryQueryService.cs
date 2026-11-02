using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Services;

public class TelemetryQueryService : ITelemetryQueryService
{
    private readonly ILapRepository _lapRepository;
    private readonly ISessionRepository _sessionRepository;

    public TelemetryQueryService(ILapRepository lapRepository, ISessionRepository sessionRepository)
    {
        _lapRepository = lapRepository;
        _sessionRepository = sessionRepository;
    }

    public Task<IReadOnlyList<SessionDto>> GetSessionsAsync(string? type, int? year) =>
        _sessionRepository.GetSessionsAsync(type, year);

    public async Task<IReadOnlyList<LapDto>> GetSessionLapsAsync(int sessionKey)
    {
        var laps = await _lapRepository.GetLapsBySessionKeyAsync(sessionKey);

        return laps.Select(lap => new LapDto(lap.EventId, lap.SessionKey, lap.DriverNumber, lap.LapNumber, lap.LapDuration,
            lap.Sector1Duration, lap.Sector2Duration, lap.Sector3Duration, lap.IsPitOutLap, lap.OccurredAt)).ToList();
    }
}
