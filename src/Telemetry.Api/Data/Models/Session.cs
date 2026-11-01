namespace Telemetry.Api.Data.Models;

public class Session
{
    public int SessionKey { get; set; }
    public int MeetingKey { get; set; }
    public int Year { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string? CircuitShortName { get; set; }
    public string? CountryName { get; set; }
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset DateEnd { get; set; }
    public bool IsCancelled { get; set; }
}
