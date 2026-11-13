using F1.Collector.Espn;
using F1Monitor.Contracts;

namespace F1.Collector.Mapping;

public class EspnSnapshotMapper
{
    public LiveRaceSnapshot? MapSnapshot(EspnCompetition competition, string sessionName, Guid eventId, DateTimeOffset observedAt)
    {
        var sessionType = competition.Type?.Abbreviation;
        if (sessionType == null || !string.Equals(sessionType, "Race", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(competition.Id) || competition.Date == null)
            return null;

        var status = competition.Status?.Type;

        return new LiveRaceSnapshot(eventId, "espn", $"espn:{competition.Id}", observedAt, competition.Date.Value, sessionType, sessionName, MapState(competition.Status), status?.Completed, MapDrivers(competition));
    }

    private static string MapState(EspnStatus? status)
    {
        return (status?.Type?.State, status?.Type?.Completed) switch
        {
            ("pre", false) => "pre",
            ("in", false) => "in",
            ("post", true) => "post",
            _ => "unknown"
        };
    }

    private static IReadOnlyList<LiveDriverSnapshot> MapDrivers(EspnCompetition competition)
    {
        var drivers = new List<LiveDriverSnapshot>();

        foreach (var competitor in competition.Competitors ?? [])
        {
            var athlete = competitor.Athlete;
            if (string.IsNullOrWhiteSpace(competitor.Id))
                continue;

            var fullName = athlete?.FullName ?? athlete?.ShortName ?? competitor.Id;
            var position = competitor.Order > 0 ? competitor.Order : null;

            drivers.Add(new LiveDriverSnapshot($"espn:{competitor.Id}", fullName, position));
        }

        return drivers;
    }
}
