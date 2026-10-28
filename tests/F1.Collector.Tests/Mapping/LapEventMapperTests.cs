using F1.Collector.Mapping;
using F1.Collector.OpenF1;
using F1Monitor.Contracts;

namespace F1.Collector.Tests.Mapping;

public class LapEventMapperTests
{
    [Fact]
    public void Map_ReturnsRaceEventWithLapCompletedPayload()
    {
        var lap = new OpenF1Lap(11396, 1, 1, DateTimeOffset.Parse("2026-10-25T20:03:49.706000+00:00"), 106.254, 30.372, 40.447, 35.435, false);

        var raceEvent = LapEventMapper.Map(lap);

        Assert.Equal(EventTypes.LapCompleted, raceEvent.Type);
        Assert.Equal(11396, raceEvent.SessionKey);
        Assert.Equal(1, raceEvent.DriverNumber);

        var payload = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);
        Assert.Equal(1, payload.LapNumber);
        Assert.Equal(106.254, payload.LapDuration);
        Assert.False(payload.IsPitOutLap);
    }

    [Fact]
    public void Map_SameLap_ReturnsSameEventId()
    {
        var lap = new OpenF1Lap(11396, 1, 1, DateTimeOffset.UtcNow, 106.254, 30.372, 40.447, 35.435, false);

        var first = LapEventMapper.Map(lap);
        var second = LapEventMapper.Map(lap);

        Assert.Equal(first.EventId, second.EventId);
    }

    [Fact]
    public void Map_WithSession_SetsSessionFields()
    {
        var lap = new OpenF1Lap(11396, 1, 1, DateTimeOffset.UtcNow, 106.254, 30.372, 40.447, 35.435, false);
        var session = new OpenF1Session(
            11396, 1297, 2026, "Race", "Race", "Austin", "United States",
            DateTimeOffset.Parse("2026-10-25T20:00:00+00:00"),
            DateTimeOffset.Parse("2026-10-25T22:00:00+00:00"),
            false);

        var raceEvent = LapEventMapper.Map(lap, session);

        Assert.Equal("Race", raceEvent.SessionType);
        Assert.Equal("Race", raceEvent.SessionName);
        Assert.Equal(LapEventMapper.Map(lap).EventId, raceEvent.EventId);
    }
}
