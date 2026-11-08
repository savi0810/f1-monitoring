using F1.Collector.Espn;
using F1Monitor.Contracts;

namespace F1.Collector.Mapping;

public class EspnSnapshotMapper
{
    public EspnCompetition? FindCompetition(EspnScoreboard scoreboard, string eventId, string competitionId)
    {
        var raceEvent = scoreboard.Events?.FirstOrDefault(item => item.Id == eventId);
        return raceEvent?.Competitions?.FirstOrDefault(item => item.Id == competitionId);
    }

    public string MapState(EspnStatus? status)
    {
        return (status?.Type?.State, status?.Type?.Completed) switch
        {
            ("pre", false) => "pre",
            ("in", false) => "in",
            ("post", true) => "post",
            _ => "unknown"
        };
    }

    public IReadOnlyList<LiveDriverSnapshot> MapDrivers(EspnCompetition competition)
    {
        var drivers = new List<LiveDriverSnapshot>();

        foreach (var competitor in competition.Competitors ?? [])
        {
            var athlete = competitor.Athlete;
            if (athlete == null || string.IsNullOrWhiteSpace(athlete.Id))
                continue;

            var fullName = athlete.FullName ?? athlete.ShortName ?? athlete.Id;
            var shortName = athlete.ShortName ?? athlete.FullName ?? athlete.Id;
            var position = competitor.Order > 0 ? competitor.Order : null;

            drivers.Add(new LiveDriverSnapshot($"espn:{athlete.Id}", fullName, shortName, position));
        }

        return drivers;
    }
}
