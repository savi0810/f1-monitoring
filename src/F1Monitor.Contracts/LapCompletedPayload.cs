namespace F1Monitor.Contracts;

public record LapCompletedPayload(
    int LapNumber,
    double? LapDuration,
    double? Sector1Duration,
    double? Sector2Duration,
    double? Sector3Duration,
    bool IsPitOutLap);
