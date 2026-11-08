namespace F1Monitor.Contracts;

public record LiveRaceSnapshot(
    Guid EventId,
    string Source,
    string SessionId,
    long Version,
    DateTimeOffset ObservedAt,
    DateTimeOffset ScheduledAt,
    string SessionType,
    string SessionName,
    string State,
    bool? Completed,
    string StatusText,
    IReadOnlyList<LiveDriverSnapshot> Drivers,
    string? RawState);
