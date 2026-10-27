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
}
