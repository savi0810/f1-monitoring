namespace F1.Collector.OpenF1;

public class OpenF1Client
{
    private readonly HttpClient _httpClient;

    public OpenF1Client(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
