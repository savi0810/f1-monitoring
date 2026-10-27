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

        return new OpenF1Client(httpClient);
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
}
