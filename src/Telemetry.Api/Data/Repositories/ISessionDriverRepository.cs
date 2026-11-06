using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public interface ISessionDriverRepository
{
    Task UpsertAsync(SessionDriver driver);
}
