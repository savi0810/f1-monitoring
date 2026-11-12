using F1Monitor.Contracts;

namespace Live.Api.Services;

public class RaceCompletionDetector
{
    public RaceCompleted? Detect(LiveRaceSnapshot snapshot)
    {
        if (snapshot.Source != "espn"
            || !string.Equals(snapshot.SessionType, "Race", StringComparison.OrdinalIgnoreCase)
            || snapshot.State != "post"
            || snapshot.Completed != true)
            return null;

        return new RaceCompleted(
            snapshot.EventId,
            snapshot.SessionId,
            snapshot.ObservedAt);
    }
}
