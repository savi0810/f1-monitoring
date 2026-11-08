using System.Text.Json;
using F1.Collector.Mapping;
using F1.Collector.OpenF1;
using F1Monitor.Contracts;

namespace F1.Collector.Tests.Unit;

public class SessionParticipantMapperTests
{
    [Fact]
    public void MapEvent_PreservesResultFormatsAndUsesStableId()
    {
        var driver = new OpenF1Driver(11396, 1, "Max Verstappen", "Red Bull Racing");
        var result = new OpenF1SessionResult(
            11396, 1, 1, JsonSerializer.SerializeToElement(5400.123),
            JsonSerializer.SerializeToElement(new double?[] { null, 1.234 }),
            57, false, false, false);

        var raceEvent = SessionParticipantMapper.MapEvent(driver, result);
        var payload = EventSerializer.ReadPayload<SessionDriverPayload>(raceEvent);

        Assert.Equal(EventTypes.SessionDriver, raceEvent.Type);
        Assert.Equal(11396, raceEvent.SessionKey);
        Assert.Equal(1, raceEvent.DriverNumber);
        Assert.Equal("Max Verstappen", payload.FullName);
        Assert.Equal(1, payload.Position);
        Assert.Equal("5400.123", payload.DurationJson);
        Assert.Equal("[null,1.234]", payload.GapToLeaderJson);
        Assert.Equal(raceEvent.EventId, SessionParticipantMapper.MapEvent(driver, result).EventId);
    }
}
