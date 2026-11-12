using F1Monitor.Contracts;
using Live.Api.Services;
using Xunit;

namespace Live.Api.Tests;

public class RaceCompletionDetectorTests
{
    private readonly RaceCompletionDetector _detector;

    public RaceCompletionDetectorTests()
    {
        _detector = new RaceCompletionDetector();
    }

    [Fact]
    public void Detect_CompletedRace_ReturnsCompletionWithSnapshotIdentity()
    {
        var snapshot = new LiveRaceSnapshot(
            Guid.NewGuid(), "espn", "race-1", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, "race", "Grand Prix", "post",
            true, []);

        var result = _detector.Detect(snapshot);

        Assert.Equal(
            new RaceCompleted(snapshot.EventId, snapshot.SessionId, snapshot.ObservedAt),
            result);
    }

    [Theory]
    [InlineData("other", "Race", "post", true)]
    [InlineData("espn", "Qualifying", "post", true)]
    [InlineData("espn", "Race", "in", false)]
    [InlineData("espn", "Race", "in", true)]
    [InlineData("espn", "Race", "post", false)]
    [InlineData("espn", "Race", "post", null)]
    public void Detect_SnapshotWithoutRaceCompletion_ReturnsNull(string source, string sessionType, string state, bool? completed)
    {
        var snapshot = new LiveRaceSnapshot(
            Guid.NewGuid(), source, "race-1", DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, sessionType, "Grand Prix", state,
            completed, []);

        var result = _detector.Detect(snapshot);

        Assert.Null(result);
    }
}
