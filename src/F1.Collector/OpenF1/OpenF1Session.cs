using System.Text.Json.Serialization;

namespace F1.Collector.OpenF1;

public record OpenF1Session(
    [property: JsonPropertyName("session_key")] int SessionKey,
    [property: JsonPropertyName("meeting_key")] int MeetingKey,
    [property: JsonPropertyName("year")] int Year,
    [property: JsonPropertyName("session_type")] string SessionType,
    [property: JsonPropertyName("session_name")] string SessionName,
    [property: JsonPropertyName("circuit_short_name")] string? CircuitShortName,
    [property: JsonPropertyName("country_name")] string? CountryName,
    [property: JsonPropertyName("date_start")] DateTimeOffset DateStart,
    [property: JsonPropertyName("date_end")] DateTimeOffset DateEnd,
    [property: JsonPropertyName("is_cancelled")] bool IsCancelled);
