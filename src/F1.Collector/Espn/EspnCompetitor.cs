using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnCompetitor(
    [property: JsonPropertyName("athlete")] EspnAthlete? Athlete,
    [property: JsonPropertyName("order")] int? Order);
