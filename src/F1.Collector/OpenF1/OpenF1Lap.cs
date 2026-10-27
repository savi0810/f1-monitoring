using System.Text.Json.Serialization;

namespace F1.Collector.OpenF1;

public record OpenF1Lap(
    [property: JsonPropertyName("session_key")] int SessionKey,
    [property: JsonPropertyName("driver_number")] int DriverNumber,
    [property: JsonPropertyName("lap_number")] int LapNumber,
    [property: JsonPropertyName("date_start")] DateTimeOffset? DateStart,
    [property: JsonPropertyName("lap_duration")] double? LapDuration,
    [property: JsonPropertyName("duration_sector_1")] double? DurationSector1,
    [property: JsonPropertyName("duration_sector_2")] double? DurationSector2,
    [property: JsonPropertyName("duration_sector_3")] double? DurationSector3,
    [property: JsonPropertyName("is_pit_out_lap")] bool IsPitOutLap);
