using System.Net;
using F1.Collector.OpenF1;
using F1.Collector.Tests.Infrastructure;

namespace F1.Collector.Tests.OpenF1;

public class OpenF1ClientTests
{
    private static OpenF1Client CreateClient(HttpStatusCode statusCode, string content)
    {
        var handler = new FakeHttpMessageHandler(statusCode, content);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var rateLimiter = new OpenF1RateLimiter();

        return new OpenF1Client(httpClient, rateLimiter);
    }

    [Fact]
    public async Task GetLapsAsync_ReturnsLaps()
    {
        const string json = """
        [
          {"session_key":11388,"driver_number":3,"lap_number":1,"date_start":"2026-10-11T12:03:39.077000+00:00","lap_duration":102.675,"duration_sector_1":32.691,"duration_sector_2":41.59,"duration_sector_3":28.394,"is_pit_out_lap":false}
        ]
        """;
        var client = CreateClient(HttpStatusCode.OK, json);

        var laps = await client.GetLapsAsync(11388);

        Assert.Single(laps);
        Assert.Equal(1, laps[0].LapNumber);
        Assert.Equal(102.675, laps[0].LapDuration);
        Assert.False(laps[0].IsPitOutLap);
    }

    [Fact]
    public async Task GetLapsAsync_Returns401_ThrowsLiveSessionException()
    {
        const string body = """{"detail":"Live F1 session in progress."}""";
        var client = CreateClient(HttpStatusCode.Unauthorized, body);

        await Assert.ThrowsAsync<OpenF1LiveSessionException>(() => client.GetLapsAsync(11388));
    }

    [Fact]
    public async Task GetSessionAsync_ReturnsSession()
    {
        const string json = """
        [
          {"session_key":11396,"session_type":"Race","session_name":"Race","date_start":"2026-10-25T20:00:00+00:00","date_end":"2026-10-25T22:00:00+00:00","meeting_key":1297,"circuit_key":9,"circuit_short_name":"Austin","country_key":19,"country_code":"USA","country_name":"United States","location":"Austin","gmt_offset":"-05:00:00","year":2026,"is_cancelled":false}
        ]
        """;
        var client = CreateClient(HttpStatusCode.OK, json);

        var session = await client.GetSessionAsync(11396);

        Assert.NotNull(session);
        Assert.Equal(1297, session.MeetingKey);
        Assert.Equal("Race", session.SessionType);
        Assert.Equal("Austin", session.CircuitShortName);
        Assert.False(session.IsCancelled);
    }

    [Fact]
    public async Task GetSessionAsync_Returns404_ReturnsNull()
    {
        const string body = """{"detail":"No results found."}""";
        var client = CreateClient(HttpStatusCode.NotFound, body);

        var session = await client.GetSessionAsync(1);

        Assert.Null(session);
    }
}
