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
}
