using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public interface ISessionRepository
{
    Task UpsertSessionAsync(Session session);
    Task<IReadOnlyList<SessionDto>> GetSessionsAsync(string? type, int? year);
}
