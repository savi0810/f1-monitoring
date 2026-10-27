namespace F1Monitor.Contracts.Tests;

public class EventSerializerTests
{
    private static RaceEvent NewEvent() => new(
        Guid.NewGuid(),
        EventTypes.LapCompleted,
        11388,
        3,
        new DateTimeOffset(2026, 10, 11, 12, 3, 39, TimeSpan.Zero),
        EventSerializer.WritePayload(new LapCompletedPayload(5, 98.4, 30.1, 40.2, 28.1, false)));

    [Fact]
    public void SerializeThenDeserialize_RestoresEvent()
    {
        var original = NewEvent();

        var restored = EventSerializer.Deserialize(EventSerializer.Serialize(original));

        Assert.Equal(original.EventId, restored.EventId);
        Assert.Equal(original.Type, restored.Type);
        Assert.Equal(original.SessionKey, restored.SessionKey);
        Assert.Equal(original.DriverNumber, restored.DriverNumber);
        Assert.Equal(original.OccurredAt, restored.OccurredAt);
    }

    [Fact]
    public void ReadPayload_ReturnsLapData()
    {
        var raceEvent = EventSerializer.Deserialize(EventSerializer.Serialize(NewEvent()));

        var lap = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);

        Assert.Equal(5, lap.LapNumber);
        Assert.Equal(98.4, lap.LapDuration);
        Assert.Equal(30.1, lap.Sector1Duration);
        Assert.False(lap.IsPitOutLap);
    }

    [Fact]
    public void Serialize_UsesCamelCaseNames()
    {
        var json = EventSerializer.Serialize(NewEvent());

        Assert.Contains("\"eventId\"", json);
        Assert.Contains("\"sessionKey\"", json);
        Assert.Contains("\"lapNumber\"", json);
        Assert.DoesNotContain("\"EventId\"", json);
    }
}
