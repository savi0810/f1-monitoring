using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public interface ISessionRepository
{
    Task UpsertSessionAsync(Session session);
    Task<Session?> GetByKeyAsync(int sessionKey, CancellationToken cancellationToken = default);
    Task<bool> TryBeginImportAsync(
        int sessionKey,
        Guid importId,
        CancellationToken cancellationToken = default);
    Task<bool> MarkImportFailedAsync(
        int sessionKey,
        Guid importId,
        string error,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(string? type, int? year);
}
