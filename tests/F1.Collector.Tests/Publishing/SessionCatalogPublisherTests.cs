using System.Net;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
using F1.Collector.Tests.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
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
        var publisher = new SessionCatalogPublisher(openF1Client, new EventPublisher(channel), NullLogger<SessionCatalogPublisher>.Instance);

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

    [Fact]
    public async Task PublishCatalogAsync_SkipsCancelledSessions()
    {
        const string sessionsJson = """
        [
          {"session_key":9079,"session_type":"Practice","session_name":"Practice 1","date_start":"2023-03-03T11:30:00+00:00","date_end":"2023-03-03T12:30:00+00:00","meeting_key":1141,"circuit_short_name":"Sakhir","country_name":"Bahrain","year":2023,"is_cancelled":true}
        ]
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, sessionsJson);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var openF1Client = new OpenF1Client(httpClient, new OpenF1RateLimiter());
        var channel = Substitute.For<IChannel>();
        var publisher = new SessionCatalogPublisher(openF1Client, new EventPublisher(channel), NullLogger<SessionCatalogPublisher>.Instance);

        await publisher.PublishCatalogAsync();

        await channel.DidNotReceive().BasicPublishAsync(
            exchange: Arg.Any<string>(),
            routingKey: Arg.Any<string>(),
            mandatory: Arg.Any<bool>(),
            basicProperties: Arg.Any<BasicProperties>(),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishCatalogAsync_DeclaresSessionsQueue()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound, """{"detail":"No results found."}""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var openF1Client = new OpenF1Client(httpClient, new OpenF1RateLimiter());
        var channel = Substitute.For<IChannel>();
        var publisher = new SessionCatalogPublisher(openF1Client, new EventPublisher(channel), NullLogger<SessionCatalogPublisher>.Instance);

        await publisher.PublishCatalogAsync();

        await channel.Received(1).QueueDeclareAsync(
            queue: "sessions.available",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: Arg.Any<CancellationToken>());
        await channel.DidNotReceive().BasicPublishAsync(
            exchange: Arg.Any<string>(),
            routingKey: Arg.Any<string>(),
            mandatory: Arg.Any<bool>(),
            basicProperties: Arg.Any<BasicProperties>(),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_OpenF1Fails_DoesNotThrow()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "boom");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var openF1Client = new OpenF1Client(httpClient, new OpenF1RateLimiter());
        var channel = Substitute.For<IChannel>();
        var publisher = new SessionCatalogPublisher(openF1Client, new EventPublisher(channel), NullLogger<SessionCatalogPublisher>.Instance);

        await publisher.StartAsync(CancellationToken.None);

        var exception = await Record.ExceptionAsync(() => publisher.ExecuteTask!);
        Assert.Null(exception);
    }
}
