namespace F1Monitor.Contracts;

public record LiveSourceStatus(
    bool IsConnected,
    int? SessionKey,
    DateTimeOffset ReportedAt);
