namespace F1Monitor.Contracts;

public record TimingUpdatedPayload(
    long Version,
    IReadOnlyList<TimingDriverPayload> Drivers);

public record TimingDriverPayload(
    int DriverNumber,
    int? Position,
    int? LapNumber,
    double? LastLapSeconds,
    string? GapToLeader);
