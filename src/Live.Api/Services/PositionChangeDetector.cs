using F1Monitor.Contracts;

namespace Live.Api.Services;

public class PositionChangeDetector
{
    public IReadOnlyList<DriverPositionChange> Detect(LiveRaceSnapshot? previous, LiveRaceSnapshot current)
    {
        if (previous == null
            || previous.Source != "espn"
            || current.Source != "espn"
            || previous.SessionId != current.SessionId
            || previous.State != "in"
            || current.State != "in"
            || previous.Completed == true
            || current.Completed == true
            || current.ObservedAt <= previous.ObservedAt)
            return [];

        var previousDrivers = previous.Drivers.ToDictionary(driver => driver.DriverId, StringComparer.Ordinal);
        var changes = new List<DriverPositionChange>();

        foreach (var driver in current.Drivers)
        {
            if (driver.Position is not int currentPosition
                || currentPosition <= 0
                || !previousDrivers.TryGetValue(driver.DriverId, out var previousDriver)
                || previousDriver.Position is not int previousPosition
                || previousPosition <= 0
                || previousPosition == currentPosition)
                continue;

            changes.Add(new DriverPositionChange(
                driver.DriverId,
                driver.FullName,
                previousPosition,
                currentPosition));
        }

        return changes;
    }
}
