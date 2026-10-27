using System.Net;
using System.Net.Http.Json;

namespace F1.Collector.OpenF1;

public class OpenF1Client
{
    private readonly HttpClient _httpClient;
    private readonly OpenF1RateLimiter _rateLimiter;

    public OpenF1Client(HttpClient httpClient, OpenF1RateLimiter rateLimiter)
    {
        _httpClient = httpClient;
        _rateLimiter = rateLimiter;
    }

    public async Task<IReadOnlyList<OpenF1Lap>> GetLapsAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        await _rateLimiter.WaitAsync(cancellationToken);

        try
        {
            var laps = await _httpClient.GetFromJsonAsync<List<OpenF1Lap>>(
                $"laps?session_key={sessionKey}", cancellationToken);

            return laps ?? [];
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new OpenF1LiveSessionException("OpenF1 free access is restricted while a live session is in progress.");
        }
    }
}
