using System.Text.Json;
using System.Text.Json.Serialization;

namespace F1.Collector.OpenF1;

public record OpenF1SessionResult(
    [property: JsonPropertyName("session_key")] int SessionKey,
    [property: JsonPropertyName("driver_number")] int DriverNumber,
    [property: JsonPropertyName("position")] int? Position,
    [property: JsonPropertyName("duration")] JsonElement Duration,
    [property: JsonPropertyName("gap_to_leader")] JsonElement GapToLeader,
    [property: JsonPropertyName("number_of_laps")] int? NumberOfLaps,
    [property: JsonPropertyName("dnf")] bool? DidNotFinish,
    [property: JsonPropertyName("dns")] bool? DidNotStart,
    [property: JsonPropertyName("dsq")] bool? Disqualified);
