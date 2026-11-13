using F1Monitor.Contracts;
using Live.Api.Services;
using Xunit;

namespace Live.Api.Tests;

public class PositionChangeDetectorTests
{
    private readonly PositionChangeDetector _detector;

    public PositionChangeDetectorTests()
    {
        _detector = new PositionChangeDetector();
    }

    [Fact]
    public void Detect_ChangedPositions_ReturnsOnlyChangedDrivers()
    {
        var previous = new LiveRaceSnapshot(
            Guid.NewGuid(), "espn", "race", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, "Race", "Grand Prix", "in", false,
            [
                new("1", "Max Verstappen", 2),
                new("4", "Lando Norris", 1),
                new("16", "Charles Leclerc", 3)
            ]);
        var current = previous with
        {
            EventId = Guid.NewGuid(),
            ObservedAt = previous.ObservedAt.AddSeconds(1),
            Drivers =
            [
                new("1", "Max Verstappen", 1),
                new("4", "Lando Norris", 2),
                new("16", "Charles Leclerc", 3)
            ]
        };

        var changes = _detector.Detect(previous, current);

        Assert.Collection(changes,
            change => Assert.Equal(new DriverPositionChange("1", "Max Verstappen", 2, 1), change),
            change => Assert.Equal(new DriverPositionChange("4", "Lando Norris", 1, 2), change));
    }

    [Theory]
    [InlineData("espn", "other", "in", false, 1)]
    [InlineData("other", "race", "in", false, 1)]
    [InlineData("espn", "race", "pre", false, 1)]
    [InlineData("espn", "race", "post", true, 1)]
    [InlineData("espn", "race", "in", true, 1)]
    [InlineData("espn", "race", "in", false, 0)]
    [InlineData("espn", "race", "in", false, -1)]
    public void Detect_IncomparableSnapshots_ReturnsNoChanges(string source, string sessionId, string state, bool completed, int seconds)
    {
        var previous = new LiveRaceSnapshot(
            Guid.NewGuid(), "espn", "race", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, "Race", "Grand Prix", "in", false,
            [new("1", "Max Verstappen", 2)]);
        var current = previous with
        {
            EventId = Guid.NewGuid(),
            Source = source,
            SessionId = sessionId,
            State = state,
            Completed = completed,
            ObservedAt = previous.ObservedAt.AddSeconds(seconds),
            Drivers = [new("1", "Max Verstappen", 1)]
        };

        var changes = _detector.Detect(previous, current);

        Assert.Empty(changes);
    }
}
