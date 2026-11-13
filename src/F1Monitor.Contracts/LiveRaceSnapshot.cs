namespace F1Monitor.Contracts;

public record LiveRaceSnapshot(
    Guid EventId,
    string Source,
    string SessionId,
    DateTimeOffset ObservedAt,
    DateTimeOffset ScheduledAt,
    string SessionType,
    string SessionName,
    string State,
    bool? Completed,
    IReadOnlyList<LiveDriverSnapshot> Drivers,
    TimeSpan? ExpectedUpdateInterval = null);
