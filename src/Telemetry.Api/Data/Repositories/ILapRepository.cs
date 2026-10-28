using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public interface ILapRepository
{
    Task<bool> AddLapAsync(Lap lap);
    Task<IReadOnlyList<RaceSummary>> GetRacesAsync();
    Task<IReadOnlyList<Lap>> GetLapsBySessionKeyAsync(int sessionKey);
}
