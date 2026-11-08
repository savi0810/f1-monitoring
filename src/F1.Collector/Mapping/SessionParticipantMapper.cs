using System.Text.Json;
using F1.Collector.OpenF1;
using F1Monitor.Contracts;

namespace F1.Collector.Mapping;

public static class SessionParticipantMapper
{
    public static IReadOnlyList<(OpenF1Driver Driver, OpenF1SessionResult? Result)> Map(IReadOnlyList<OpenF1Driver> drivers, IReadOnlyList<OpenF1SessionResult> results)
    {
        var resultsByDriver = results.ToLookup(result => result.DriverNumber);

        return drivers
            .Select(driver => (driver, resultsByDriver[driver.DriverNumber].FirstOrDefault()))
            .ToList();
    }

    public static RaceEvent MapEvent(OpenF1Driver driver, OpenF1SessionResult? result)
    {
        var payload = new SessionDriverPayload(
            driver.FullName,
            driver.TeamName,
            result?.Position,
            result?.NumberOfLaps,
            result is null || result.Duration.ValueKind == JsonValueKind.Undefined ? null : result.Duration.GetRawText(),
            result is null || result.GapToLeader.ValueKind == JsonValueKind.Undefined ? null : result.GapToLeader.GetRawText(),
            result?.DidNotFinish,
            result?.DidNotStart,
            result?.Disqualified);

        return new RaceEvent(
            EventIdFactory.Create($"{EventTypes.SessionDriver}:{driver.SessionKey}:{driver.DriverNumber}"),
            EventTypes.SessionDriver,
            driver.SessionKey,
            driver.DriverNumber,
            null,
            EventSerializer.WritePayload(payload));
    }
}
