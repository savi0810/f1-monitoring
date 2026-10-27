using System.Threading.RateLimiting;

namespace F1.Collector.OpenF1;

public class OpenF1RateLimiter : IAsyncDisposable
{
    private readonly RateLimiter _perSecondLimiter;
    private readonly RateLimiter _perMinuteLimiter;

    public OpenF1RateLimiter(int requestsPerSecond = 3, int requestsPerMinute = 30)
    {
        _perSecondLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = requestsPerSecond,
            Window = TimeSpan.FromSeconds(1),
            SegmentsPerWindow = 1,
            QueueLimit = int.MaxValue
        });

        _perMinuteLimiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = requestsPerMinute,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = int.MaxValue
        });
    }

    public async ValueTask DisposeAsync()
    {
        _perSecondLimiter.Dispose();
        _perMinuteLimiter.Dispose();
    }
}
