using F1.Collector.OpenF1;

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
}
