using Telemetry.Api.Data.DTOs;

namespace Telemetry.Api.Services;

public interface ISessionImportService
{
    Task<SessionImportDto?> StartImportAsync(
        int sessionKey,
        CancellationToken cancellationToken = default);
}
