namespace Live.Api.Data.DTOs;

public record LiveDriverDto(
    int DriverNumber,
    int? Position,
    int? LapNumber,
    double? LastLapSeconds,
    string? GapToLeader);
