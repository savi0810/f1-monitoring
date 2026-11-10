namespace F1Monitor.Contracts;

public record PositionAlertTriggered(
    Guid AlertId,
    Guid UserId,
    Guid SubscriptionId,
    Guid EventId,
    string SessionId,
    string DriverId,
    string DriverName,
    int PreviousPosition,
    int CurrentPosition,
    DateTimeOffset ObservedAt);
