namespace F1.Collector.Espn;

public class EspnClient
{
    private readonly HttpClient _httpClient;

    public EspnClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
