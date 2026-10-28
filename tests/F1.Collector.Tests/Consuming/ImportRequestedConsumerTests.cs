using System.Net;
using F1.Collector.Consuming;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
using F1.Collector.Tests.Infrastructure;
using F1Monitor.Contracts;
using NSubstitute;
using RabbitMQ.Client;

namespace F1.Collector.Tests.Consuming;

public class ImportRequestedConsumerTests
{
    [Fact]
    public async Task HandleImportRequestedAsync_PublishesOneEventPerLap()
    {
        const string json = """
        [
          {"session_key":11396,"driver_number":1,"lap_number":1,"date_start":"2026-10-25T20:03:49.706000+00:00","lap_duration":106.254,"duration_sector_1":30.372,"duration_sector_2":40.447,"duration_sector_3":35.435,"is_pit_out_lap":false},
          {"session_key":11396,"driver_number":1,"lap_number":2,"date_start":"2026-10-25T20:05:35.960000+00:00","lap_duration":105.1,"duration_sector_1":30.1,"duration_sector_2":40.1,"duration_sector_3":34.9,"is_pit_out_lap":false}
        ]
        """;
        const string sessionJson = """
        [{"session_key":11396,"session_type":"Race","session_name":"Race","date_start":"2026-10-25T20:00:00+00:00","date_end":"2026-10-25T22:00:00+00:00","meeting_key":1297,"circuit_short_name":"Austin","country_name":"United States","year":2026,"is_cancelled":false}]
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, json)
            .WithRoute("sessions", HttpStatusCode.OK, sessionJson);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var openF1Client = new OpenF1Client(httpClient, new OpenF1RateLimiter());
        var channel = Substitute.For<IChannel>();
        var eventPublisher = new EventPublisher(channel);
        var consumer = new ImportRequestedConsumer(channel, openF1Client, eventPublisher);

        await consumer.HandleImportRequestedAsync(new ImportRequested(11396));

        await channel.Received(2).BasicPublishAsync(
            exchange: ExchangeNames.F1Events,
            routingKey: "history.lap_completed",
            mandatory: true,
            basicProperties: Arg.Any<BasicProperties>(),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleImportRequestedAsync_UnknownSession_PublishesNothing()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.NotFound, """{"detail":"No results found."}""");
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.openf1.org/v1/") };
        var openF1Client = new OpenF1Client(httpClient, new OpenF1RateLimiter());
        var channel = Substitute.For<IChannel>();
        var consumer = new ImportRequestedConsumer(channel, openF1Client, new EventPublisher(channel));

        await consumer.HandleImportRequestedAsync(new ImportRequested(1));

        await channel.DidNotReceive().BasicPublishAsync(
            exchange: Arg.Any<string>(),
            routingKey: Arg.Any<string>(),
            mandatory: Arg.Any<bool>(),
            basicProperties: Arg.Any<BasicProperties>(),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
