using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnCompetition(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("date")] DateTimeOffset? Date,
    [property: JsonPropertyName("status")] EspnStatus? Status,
    [property: JsonPropertyName("competitors")] IReadOnlyList<EspnCompetitor>? Competitors);
