using F1.Collector.Mapping;
using F1.Collector.OpenF1;

namespace F1.Collector.Tests.Mapping;

public class SessionAvailableMapperTests
{
    [Fact]
    public void Map_CopiesAllSessionFields()
    {
        var session = new OpenF1Session(
            11731, 1308, 2026, "Race", "Race", "Kuala Lumpur", "Bahrain",
            DateTimeOffset.Parse("2026-10-04T07:00:00+00:00"),
            DateTimeOffset.Parse("2026-10-04T09:00:00+00:00"),
            false);

        var available = SessionAvailableMapper.Map(session);

        Assert.Equal(11731, available.SessionKey);
        Assert.Equal(1308, available.MeetingKey);
        Assert.Equal(2026, available.Year);
        Assert.Equal("Race", available.SessionType);
        Assert.Equal("Race", available.SessionName);
        Assert.Equal("Kuala Lumpur", available.CircuitShortName);
        Assert.Equal("Bahrain", available.CountryName);
        Assert.Equal(session.DateStart, available.DateStart);
        Assert.Equal(session.DateEnd, available.DateEnd);
        Assert.False(available.IsCancelled);
    }
}
