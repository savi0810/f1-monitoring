namespace Telemetry.Api.Data.Models;

public class Lap
{
    public Guid EventId { get; set; }
    public int SessionKey { get; set; }
    public int? DriverNumber { get; set; }
    public int LapNumber { get; set; }
    public double? LapDuration { get; set; }
    public double? Sector1Duration { get; set; }
    public double? Sector2Duration { get; set; }
    public double? Sector3Duration { get; set; }
    public bool IsPitOutLap { get; set; }
    public DateTimeOffset? OccurredAt { get; set; }
}
