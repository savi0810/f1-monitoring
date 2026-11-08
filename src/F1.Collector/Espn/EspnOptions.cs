namespace F1.Collector.Espn;

public class EspnOptions
{
    public bool Enabled { get; set; }
    public string EventId { get; set; } = "";
    public string CompetitionId { get; set; } = "";
    public string Mode { get; set; } = "Auto";
    public int PollIntervalMilliseconds { get; set; } = 500;
    public int SchedulePollIntervalSeconds { get; set; } = 180;
    public int RequestTimeoutSeconds { get; set; } = 10;
    public int FastPollingLeadMinutes { get; set; } = 10;
    public int PostFinishMinutes { get; set; } = 5;
    public DateTimeOffset? FastPollingStartsAt { get; set; }
    public bool CaptureEnabled { get; set; }
}
