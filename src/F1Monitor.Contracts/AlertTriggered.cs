namespace F1Monitor.Contracts;

public record AlertTriggered(
    Guid AlertId,
    Guid UserId,
    Guid RuleId,
    Guid EventId,
    int DriverNumber,
    double LapTimeSeconds);
