namespace F1.Collector.Espn;

public class EspnClient
{
    private readonly HttpClient _httpClient;

    public EspnClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> GetScoreboardAsync(CancellationToken cancellationToken = default)
    {
        return _httpClient.GetAsync("scoreboard?dates=20261120", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }
}
