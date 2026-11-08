using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnCompetitor(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("athlete")] EspnAthlete? Athlete,
    [property: JsonPropertyName("order")] int? Order);
