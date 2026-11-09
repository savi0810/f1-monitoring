namespace F1.Collector.Espn;

public class EspnPollingSchedule
{
    public TimeSpan? GetInterval(EspnOptions options, DateTimeOffset now, DateTimeOffset? scheduledAt, DateTimeOffset? finishedAt)
    {
        if (!options.Enabled || options.Mode.Equals("Stop", StringComparison.OrdinalIgnoreCase))
            return null;

        var fast = TimeSpan.FromMilliseconds(options.PollIntervalMilliseconds);
        var slow = TimeSpan.FromSeconds(options.SchedulePollIntervalSeconds);

        if (options.Mode.Equals("Start", StringComparison.OrdinalIgnoreCase))
            return fast;

        if (finishedAt != null)
            return now < finishedAt.Value.AddMinutes(options.PostFinishMinutes) ? fast : slow;

        var startsAt = options.FastPollingStartsAt ?? scheduledAt?.AddMinutes(-options.FastPollingLeadMinutes);
        return startsAt != null && now >= startsAt.Value ? fast : slow;
    }
}
