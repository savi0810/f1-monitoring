using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnStatusType(
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("completed")] bool? Completed,
    [property: JsonPropertyName("description")] string? Description);
