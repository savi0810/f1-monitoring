using Live.Api.Data.DTOs;
using Live.Api.Data.Models;

namespace Live.Api.Mapping;

public static class LiveSnapshotMapper
{
    public static LiveSnapshotDto ToDto(LiveSnapshot snapshot) => new(
        snapshot.SessionKey,
        snapshot.Version,
        snapshot.UpdatedAt,
        snapshot.Drivers.Select(driver => new LiveDriverDto(
            driver.DriverNumber,
            driver.Position,
            driver.LapNumber,
            driver.LastLapSeconds,
            driver.GapToLeader)).ToList());
}
