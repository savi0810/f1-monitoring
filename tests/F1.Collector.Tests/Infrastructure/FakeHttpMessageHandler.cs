using System.Net;

namespace F1.Collector.Tests.Infrastructure;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _content;
    private readonly List<(string UrlFragment, HttpStatusCode StatusCode, string Content)> _routes = [];

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _statusCode = statusCode;
        _content = content;
    }

    public FakeHttpMessageHandler WithRoute(string urlFragment, HttpStatusCode statusCode, string content)
    {
        _routes.Add((urlFragment, statusCode, content));
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.PathAndQuery;
        var route = _routes.FirstOrDefault(r => url.Contains(r.UrlFragment));
        var matched = route.UrlFragment is not null;

        var response = new HttpResponseMessage(matched ? route.StatusCode : _statusCode)
        {
            Content = new StringContent(matched ? route.Content : _content)
        };

        return Task.FromResult(response);
    }
}
