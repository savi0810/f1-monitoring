using System.Text.Json.Serialization;

namespace F1.Collector.OpenF1;

public record OpenF1Driver(
    [property: JsonPropertyName("session_key")] int SessionKey,
    [property: JsonPropertyName("driver_number")] int DriverNumber,
    [property: JsonPropertyName("full_name")] string? FullName,
    [property: JsonPropertyName("team_name")] string? TeamName);
