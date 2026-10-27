using F1.Collector.Mapping;

namespace F1.Collector.Tests.Mapping;

public class EventIdFactoryTests
{
    [Fact]
    public void Create_SameKey_ReturnsSameGuid()
    {
        var first = EventIdFactory.Create("lap_completed:11396:1:1");
        var second = EventIdFactory.Create("lap_completed:11396:1:1");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Create_DifferentKey_ReturnsDifferentGuid()
    {
        var first = EventIdFactory.Create("lap_completed:11396:1:1");
        var second = EventIdFactory.Create("lap_completed:11396:1:2");

        Assert.NotEqual(first, second);
    }
}
