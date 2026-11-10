namespace F1Monitor.Contracts;

public record DriverPositionChange(
    string DriverId,
    string DriverName,
    int PreviousPosition,
    int CurrentPosition);
