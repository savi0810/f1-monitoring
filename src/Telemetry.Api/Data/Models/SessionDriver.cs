namespace Telemetry.Api.Data.Models;

public class SessionDriver
{
    public int SessionKey { get; set; }
    public int DriverNumber { get; set; }
    public string? FullName { get; set; }
    public string? TeamName { get; set; }
}
