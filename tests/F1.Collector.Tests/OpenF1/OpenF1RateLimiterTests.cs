using System.Diagnostics;
using F1.Collector.OpenF1;

namespace F1.Collector.Tests.OpenF1;

public class OpenF1RateLimiterTests
{
    [Fact]
    public async Task WaitAsync_AllowsRequestWithinLimit()
    {
        var limiter = new OpenF1RateLimiter(requestsPerSecond: 3, requestsPerMinute: 30);
        var stopwatch = Stopwatch.StartNew();

        await limiter.WaitAsync();

        Assert.True(stopwatch.Elapsed < TimeSpan.FromMilliseconds(500));

        await limiter.DisposeAsync();
    }
}
