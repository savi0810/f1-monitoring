namespace F1Monitor.Contracts;

public record DriverPositionsChanged(
    Guid EventId,
    string SessionId,
    DateTimeOffset ObservedAt,
    IReadOnlyList<DriverPositionChange> Changes);
