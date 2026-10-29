using System.Net;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
using F1.Collector.Tests.Infrastructure;
using NSubstitute;
using RabbitMQ.Client;

namespace F1.Collector.Tests.Publishing;

public class SessionCatalogPublisherTests
{
    [Fact]
    public async Task PublishCatalogAsync_PublishesSessionsOfEveryYear()
    {
        const string sessionsJson = """
        [{"session_key":11731,"session_type":"Race","session_name":"Race","date_start":"2026-10-04T07:00:00+00:00","date_end":"2026-10-04T09:00:00+00:00","meeting_key":1308,"circuit_short_name":"Kuala Lumpur","country_name":"Bahrain","year":2026,"is_cancelled":false}]
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, sessionsJson);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var openF1Client = new OpenF1Client(httpClient, new OpenF1RateLimiter());
        var channel = Substitute.For<IChannel>();
        var publisher = new SessionCatalogPublisher(openF1Client, new EventPublisher(channel));

        await publisher.PublishCatalogAsync();

        var years = DateTime.UtcNow.Year - 2023 + 1;
        await channel.Received(years).BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "sessions.available",
            mandatory: true,
            basicProperties: Arg.Any<BasicProperties>(),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
