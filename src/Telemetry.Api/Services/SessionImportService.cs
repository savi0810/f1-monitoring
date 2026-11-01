using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Publishing;

namespace Telemetry.Api.Services;

public class SessionImportService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ImportRequestPublisher _importRequestPublisher;

    public SessionImportService(
        ISessionRepository sessionRepository,
        ImportRequestPublisher importRequestPublisher)
    {
        _sessionRepository = sessionRepository;
        _importRequestPublisher = importRequestPublisher;
    }
}
