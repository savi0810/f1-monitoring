using System.Text.Json.Serialization;

namespace F1.Collector.Espn;

public record EspnStatus(
    [property: JsonPropertyName("type")] EspnStatusType? Type);
