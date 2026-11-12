using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Mapping;

namespace Telemetry.Api.Services;

public class TelemetryQueryService : ITelemetryQueryService
{
    private readonly ILapRepository _lapRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ISessionDriverRepository _sessionDriverRepository;

    public TelemetryQueryService(ILapRepository lapRepository, ISessionRepository sessionRepository, ISessionDriverRepository sessionDriverRepository)
    {
        _lapRepository = lapRepository;
        _sessionRepository = sessionRepository;
        _sessionDriverRepository = sessionDriverRepository;
    }

    public Task<IReadOnlyList<SessionCatalogYearDto>> GetCatalogAsync(string? type, int? year) =>
        _sessionRepository.GetCatalogAsync(type, year);

    public async Task<bool> SessionExistsAsync(int sessionKey) =>
        await _sessionRepository.GetByKeyAsync(sessionKey) is not null;

    public async Task<IReadOnlyList<SessionResultDto>> GetSessionResultsAsync(int sessionKey)
    {
        var session = await _sessionRepository.GetByKeyAsync(sessionKey)
            ?? throw new InvalidOperationException($"Session {sessionKey} was not found.");
        var drivers = await _sessionDriverRepository.GetBySessionKeyAsync(sessionKey);
        var laps = await _lapRepository.GetLapsBySessionKeyAsync(sessionKey);
        var lapsByDriver = laps
            .Where(lap => lap.DriverNumber.HasValue)
            .ToLookup(lap => lap.DriverNumber!.Value);

        return drivers
            .Select(driver => SessionResultMapper.Map(driver, session, lapsByDriver[driver.DriverNumber].ToList()))
            .ToList();
    }
}
