using System.Text.Json;

namespace F1Monitor.Contracts;

public static class EventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(RaceEvent raceEvent) =>
        JsonSerializer.Serialize(raceEvent, Options);

    public static RaceEvent Deserialize(string json) =>
        JsonSerializer.Deserialize<RaceEvent>(json, Options)
            ?? throw new JsonException("Event is empty.");

    public static JsonElement WritePayload<T>(T payload) =>
        JsonSerializer.SerializeToElement(payload, Options);

    public static T ReadPayload<T>(RaceEvent raceEvent) =>
        raceEvent.Payload.Deserialize<T>(Options)
            ?? throw new JsonException("Payload is empty.");

    public static string Serialize<T>(T message) =>
        JsonSerializer.Serialize(message, Options);

    public static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options)
            ?? throw new JsonException("Message is empty.");
}
