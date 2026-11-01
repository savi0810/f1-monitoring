using Telemetry.Api.Data.DTOs;

namespace Telemetry.Api.Services;

public interface ITelemetryQueryService
{
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(string? type, int? year);
    Task<IReadOnlyList<LapDto>> GetSessionLapsAsync(int sessionKey);
}
