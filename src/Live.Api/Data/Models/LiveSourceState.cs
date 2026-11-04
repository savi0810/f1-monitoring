namespace Live.Api.Data.Models;

public class LiveSourceState
{
    public bool IsConnected { get; set; }
    public int? SessionKey { get; set; }
    public DateTimeOffset ReportedAt { get; set; }
}
