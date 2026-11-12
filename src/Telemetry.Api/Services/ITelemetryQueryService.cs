using Telemetry.Api.Data.DTOs;

namespace Telemetry.Api.Services;

public interface ITelemetryQueryService
{
    Task<IReadOnlyList<SessionCatalogYearDto>> GetCatalogAsync(string? type, int? year);
    Task<bool> SessionExistsAsync(int sessionKey);
    Task<IReadOnlyList<SessionResultDto>> GetSessionResultsAsync(int sessionKey);
}
