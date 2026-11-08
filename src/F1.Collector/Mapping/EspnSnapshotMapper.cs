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

    public LiveRaceSnapshot? MapSnapshot(EspnCompetition competition, string sessionName, Guid eventId, long version, DateTimeOffset observedAt)
    {
        var sessionType = competition.Type?.Abbreviation;
        if (sessionType == null || !string.Equals(sessionType, "Race", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(competition.Id) || competition.Date == null)
            return null;

        var status = competition.Status?.Type;

        return new LiveRaceSnapshot(eventId, "espn", $"espn:{competition.Id}", version, observedAt, competition.Date.Value, sessionType, sessionName, MapState(competition.Status), status?.Completed, status?.Description ?? "", MapDrivers(competition), status?.State);
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
            if (string.IsNullOrWhiteSpace(competitor.Id))
                continue;

            var fullName = athlete?.FullName ?? athlete?.ShortName ?? competitor.Id;
            var shortName = athlete?.ShortName ?? athlete?.FullName ?? competitor.Id;
            var position = competitor.Order > 0 ? competitor.Order : null;

            drivers.Add(new LiveDriverSnapshot($"espn:{competitor.Id}", fullName, shortName, position));
        }

        return drivers;
    }
}
