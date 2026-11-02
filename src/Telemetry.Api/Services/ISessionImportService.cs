using Telemetry.Api.Data.DTOs;

namespace Telemetry.Api.Services;

public interface ISessionImportService
{
    Task<SessionImportDto?> StartImportAsync(
        int sessionKey,
        CancellationToken cancellationToken = default);
    Task<(bool SessionExists, SessionImportDto? Import)> GetImportStatusAsync(int sessionKey, CancellationToken cancellationToken = default);
}
