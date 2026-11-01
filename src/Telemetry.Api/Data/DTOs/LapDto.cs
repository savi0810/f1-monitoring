namespace Telemetry.Api.Data.DTOs;

public record LapDto(Guid EventId, int SessionKey, int? DriverNumber, int LapNumber, double? LapDuration,
    double? Sector1Duration, double? Sector2Duration, double? Sector3Duration, bool IsPitOutLap, DateTimeOffset? OccurredAt);
