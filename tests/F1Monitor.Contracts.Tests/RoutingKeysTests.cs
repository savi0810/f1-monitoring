namespace F1Monitor.Contracts.Tests;

public class RoutingKeysTests
{
    [Fact]
    public void For_CombinesSourceAndEventType()
    {
        Assert.Equal("live.lap_completed", RoutingKeys.For(RoutingKeys.Live, EventTypes.LapCompleted));
        Assert.Equal("history.lap_completed", RoutingKeys.For(RoutingKeys.History, EventTypes.LapCompleted));
    }
}
