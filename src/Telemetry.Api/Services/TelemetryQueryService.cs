using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Services;

public class TelemetryQueryService
{
    private readonly ILapRepository _lapRepository;
    private readonly ISessionRepository _sessionRepository;

    public TelemetryQueryService(ILapRepository lapRepository, ISessionRepository sessionRepository)
    {
        _lapRepository = lapRepository;
        _sessionRepository = sessionRepository;
    }
}
