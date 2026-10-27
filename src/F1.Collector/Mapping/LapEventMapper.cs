using F1.Collector.OpenF1;
using F1Monitor.Contracts;

namespace F1.Collector.Mapping;

public static class LapEventMapper
{
    public static RaceEvent Map(OpenF1Lap lap)
    {
        var eventId = EventIdFactory.Create(
            $"{EventTypes.LapCompleted}:{lap.SessionKey}:{lap.DriverNumber}:{lap.LapNumber}");

        var payload = new LapCompletedPayload(
            lap.LapNumber,
            lap.LapDuration,
            lap.DurationSector1,
            lap.DurationSector2,
            lap.DurationSector3,
            lap.IsPitOutLap);

        return new RaceEvent(
            eventId,
            EventTypes.LapCompleted,
            lap.SessionKey,
            lap.DriverNumber,
            lap.DateStart ?? DateTimeOffset.UtcNow,
            EventSerializer.WritePayload(payload));
    }
}
