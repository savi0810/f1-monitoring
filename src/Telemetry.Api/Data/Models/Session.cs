using Telemetry.Api.Data.DTOs;

namespace Telemetry.Api.Data.Models;

public class Session
{
    public int SessionKey { get; set; }
    public int MeetingKey { get; set; }
    public Meeting Meeting { get; set; } = null!;
    public int Year { get; set; }
    public string SessionType { get; set; } = string.Empty;
    public string SessionName { get; set; } = string.Empty;
    public string? CircuitShortName { get; set; }
    public string? CountryName { get; set; }
    public DateTimeOffset DateStart { get; set; }
    public DateTimeOffset DateEnd { get; set; }
    public bool IsCancelled { get; set; }
    public Guid? ImportId { get; set; }
    public ImportStatus? ImportStatus { get; set; }
    public DateTimeOffset? ImportCompletedAt { get; set; }
    public string? ImportError { get; set; }
}
