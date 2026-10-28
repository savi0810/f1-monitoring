using F1Monitor.Contracts;
using Telemetry.Api.Mapping;

namespace Telemetry.Api.Tests.Unit;

public class LapMapperTests
{
    [Fact]
    public void FromRaceEvent_MapsAllFields()
    {
        var eventId = Guid.NewGuid();
        var occurredAt = DateTimeOffset.UtcNow;
        var raceEvent = new RaceEvent(eventId, EventTypes.LapCompleted, 11396, 1, occurredAt,
            EventSerializer.WritePayload(new LapCompletedPayload(5, 98.4, 30.1, 40.2, 28.1, false)));
        var payload = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);

        var lap = LapMapper.FromRaceEvent(raceEvent, payload);

        Assert.Equal(eventId, lap.EventId);
        Assert.Equal(11396, lap.SessionKey);
        Assert.Equal(1, lap.DriverNumber);
        Assert.Equal(5, lap.LapNumber);
        Assert.Equal(98.4, lap.LapDuration);
        Assert.Equal(occurredAt, lap.OccurredAt);
    }
}
