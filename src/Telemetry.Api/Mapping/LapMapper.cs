using F1Monitor.Contracts;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Mapping;

public static class LapMapper
{
    public static Lap FromRaceEvent(RaceEvent raceEvent, LapCompletedPayload payload) => new()
    {
        EventId = raceEvent.EventId,
        SessionKey = raceEvent.SessionKey,
        DriverNumber = raceEvent.DriverNumber,
        LapNumber = payload.LapNumber,
        LapDuration = payload.LapDuration,
        Sector1Duration = payload.Sector1Duration,
        Sector2Duration = payload.Sector2Duration,
        Sector3Duration = payload.Sector3Duration,
        IsPitOutLap = payload.IsPitOutLap,
        OccurredAt = raceEvent.OccurredAt
    };
}
