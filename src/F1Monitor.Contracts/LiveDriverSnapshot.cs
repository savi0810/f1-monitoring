namespace F1Monitor.Contracts;

public record LiveDriverSnapshot(
    string DriverId,
    string FullName,
    int? Position);
