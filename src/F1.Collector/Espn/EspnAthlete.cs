using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnAthlete(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("fullName")] string? FullName,
    [property: JsonPropertyName("shortName")] string? ShortName);
