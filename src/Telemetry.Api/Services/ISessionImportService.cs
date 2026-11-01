using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Services;

public interface ISessionImportService
{
    Task<SessionImportDto> StartImportAsync(
        Session session,
        CancellationToken cancellationToken = default);
}
