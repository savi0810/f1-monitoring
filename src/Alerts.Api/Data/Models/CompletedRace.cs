namespace Alerts.Api.Data.Models;

public class CompletedRace
{
    public CompletedRace()
    {
        SessionId = string.Empty;
        RecordedAt = DateTimeOffset.UtcNow;
    }

    public string SessionId { get; set; }
    public Guid EventId { get; set; }
    public DateTimeOffset ObservedAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
}
