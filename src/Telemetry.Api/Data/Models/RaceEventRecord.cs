namespace Telemetry.Api.Data.Models;

public class RaceEventRecord
{
    public Guid EventId { get; set; }
    public string Type { get; set; } = string.Empty;
    public int SessionKey { get; set; }
    public int? DriverNumber { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
}
