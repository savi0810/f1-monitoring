using System.Text.Json.Serialization;

namespace F1.Collector.OpenF1;

public record OpenF1Meeting(
    [property: JsonPropertyName("meeting_key")] int MeetingKey,
    [property: JsonPropertyName("meeting_name")] string MeetingName);
