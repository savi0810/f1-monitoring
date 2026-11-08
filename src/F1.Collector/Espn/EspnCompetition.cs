using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnCompetition(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("date")] DateTimeOffset? Date,
    [property: JsonPropertyName("status")] EspnStatus? Status,
    [property: JsonPropertyName("competitors")] IReadOnlyList<EspnCompetitor>? Competitors,
    [property: JsonPropertyName("type")] EspnCompetitionType? Type);

public record EspnCompetitionType(
    [property: JsonPropertyName("abbreviation")] string? Abbreviation);
