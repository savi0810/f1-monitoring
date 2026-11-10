namespace F1Monitor.Contracts;

public record RaceCompleted(
    Guid EventId,
    string SessionId,
    DateTimeOffset ObservedAt);
