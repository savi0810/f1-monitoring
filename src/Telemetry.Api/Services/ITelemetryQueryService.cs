using Telemetry.Api.Data.DTOs;

namespace Telemetry.Api.Services;

public interface ITelemetryQueryService
{
    Task<IReadOnlyList<SessionCatalogYearDto>> GetCatalogAsync(string? type, int? year);
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(string? type, int? year);
    Task<bool> SessionExistsAsync(int sessionKey);
    Task<IReadOnlyList<LapDto>> GetSessionLapsAsync(int sessionKey);
}
