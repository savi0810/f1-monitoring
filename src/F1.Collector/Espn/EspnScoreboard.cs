using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnScoreboard(
    [property: JsonPropertyName("events")] IReadOnlyList<EspnEvent>? Events);

public record EspnEvent(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("competitions")] IReadOnlyList<EspnCompetition>? Competitions);
