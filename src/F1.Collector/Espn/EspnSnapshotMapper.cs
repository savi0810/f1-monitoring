namespace F1.Collector.Espn;

public class EspnSnapshotMapper
{
    public EspnCompetition? FindCompetition(EspnScoreboard scoreboard, string eventId, string competitionId)
    {
        var raceEvent = scoreboard.Events?.FirstOrDefault(item => item.Id == eventId);
        return raceEvent?.Competitions?.FirstOrDefault(item => item.Id == competitionId);
    }
}
