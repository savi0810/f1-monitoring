using System.Text.Json;

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
    public void SerializeThenDeserialize_RestoresSessionFields()
    {
        var original = NewEvent() with { SessionType = "Race", SessionName = "Sprint" };

        var restored = EventSerializer.Deserialize(EventSerializer.Serialize(original));

        Assert.Equal("Race", restored.SessionType);
        Assert.Equal("Sprint", restored.SessionName);
    }

    [Fact]
    public void Deserialize_EventWithoutSessionFields_ReturnsNulls()
    {
        const string json = """
            {"eventId":"11111111-1111-1111-1111-111111111111","type":"lap_completed","sessionKey":11388,
             "driverNumber":3,"occurredAt":"2026-10-11T12:03:39+00:00","payload":{}}
            """;

        var restored = EventSerializer.Deserialize(json);

        Assert.Null(restored.SessionType);
        Assert.Null(restored.SessionName);
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

    [Fact]
    public void Event_WithoutDriverAndLapTime_SurvivesRoundTrip()
    {
        var original = new RaceEvent(
            Guid.NewGuid(),
            EventTypes.LapCompleted,
            11388,
            null,
            DateTimeOffset.UtcNow,
            EventSerializer.WritePayload(new LapCompletedPayload(1, null, null, null, null, true)));

        var restored = EventSerializer.Deserialize(EventSerializer.Serialize(original));
        var lap = EventSerializer.ReadPayload<LapCompletedPayload>(restored);

        Assert.Null(restored.DriverNumber);
        Assert.Null(lap.LapDuration);
        Assert.True(lap.IsPitOutLap);
    }

    [Fact]
    public void Deserialize_NullJson_Throws()
    {
        Assert.Throws<JsonException>(() => EventSerializer.Deserialize("null"));
    }

    [Fact]
    public void GenericSerializeThenDeserialize_RestoresMessage()
    {
        var original = new ImportRequested(11396);

        var restored = EventSerializer.Deserialize<ImportRequested>(EventSerializer.Serialize(original));

        Assert.Equal(original.SessionKey, restored.SessionKey);
    }

    [Fact]
    public void ReadPayload_NullPayload_Throws()
    {
        var raceEvent = new RaceEvent(
            Guid.NewGuid(),
            EventTypes.LapCompleted,
            11388,
            3,
            DateTimeOffset.UtcNow,
            EventSerializer.WritePayload<LapCompletedPayload?>(null));

        Assert.Throws<JsonException>(() => EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent));
    }
}
