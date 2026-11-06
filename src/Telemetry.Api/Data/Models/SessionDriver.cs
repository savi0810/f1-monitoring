namespace Telemetry.Api.Data.Models;

public class SessionDriver
{
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public string? FullName { get; set; }
    public string? TeamName { get; set; }
    public int? Position { get; set; }
    public int? NumberOfLaps { get; set; }
    public string? DurationJson { get; set; }
    public string? GapToLeaderJson { get; set; }
    public bool? DidNotFinish { get; set; }
    public bool? DidNotStart { get; set; }
    public bool? Disqualified { get; set; }
}
