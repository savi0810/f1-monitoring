namespace F1Monitor.Contracts;

public record SessionAvailable(
    int SessionKey,
    int MeetingKey,
    int Year,
    string SessionType,
    string SessionName,
    string? CircuitShortName,
    string? CountryName,
    DateTimeOffset DateStart,
    DateTimeOffset DateEnd,
    bool IsCancelled);
