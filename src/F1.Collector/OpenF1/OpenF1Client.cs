using System.Net;
using System.Net.Http.Json;

namespace F1.Collector.OpenF1;

public class OpenF1Client
{
    private readonly HttpClient _httpClient;

    public OpenF1Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<OpenF1Lap>> GetLapsAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
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
