namespace Live.Api.Data.Models;

public class LiveDriver
{
    public int DriverNumber { get; set; }
    public int? Position { get; set; }
    public int? LapNumber { get; set; }
    public double? LastLapSeconds { get; set; }
    public string? GapToLeader { get; set; }
}
