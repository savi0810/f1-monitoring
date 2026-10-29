namespace Telemetry.Api.Data.DTOs;

public record SessionDto(
    int SessionKey,
    int MeetingKey,
    int Year,
    string SessionType,
    string SessionName,
    string? CircuitShortName,
    string? CountryName,
    DateTimeOffset DateStart,
    DateTimeOffset DateEnd,
    bool IsCancelled,
    int LapCount);
