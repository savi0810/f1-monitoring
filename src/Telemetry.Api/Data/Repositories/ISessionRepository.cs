using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public interface ISessionRepository
{
    Task UpsertSessionAsync(Session session);
}
