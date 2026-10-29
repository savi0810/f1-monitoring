using F1.Collector.Publishing;
using F1Monitor.Contracts;
using NSubstitute;
using RabbitMQ.Client;

namespace F1.Collector.Tests.Publishing;

public class EventPublisherTests
{
    [Fact]
    public async Task DeclareExchangeAsync_DeclaresDurableTopicExchange()
    {
        var channel = Substitute.For<IChannel>();
        var publisher = new EventPublisher(channel);

        await publisher.DeclareExchangeAsync();

        await channel.Received(1).ExchangeDeclareAsync(
            exchange: "f1.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishToExchangeAsync_PublishesWithSourceAndTypeRoutingKey()
    {
        var channel = Substitute.For<IChannel>();
        var publisher = new EventPublisher(channel);
        var events = new[]
        {
            new RaceEvent(
                Guid.NewGuid(),
                EventTypes.LapCompleted,
                11396,
                1,
                DateTimeOffset.UtcNow,
                EventSerializer.WritePayload(new LapCompletedPayload(1, 100, 30, 40, 30, false)))
        };

        await publisher.PublishToExchangeAsync(events, RoutingKeys.History);

        await channel.Received(1).BasicPublishAsync(
            exchange: "f1.events",
            routingKey: "history.lap_completed",
            mandatory: true,
            basicProperties: Arg.Is<BasicProperties>(p => p.Persistent),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeclareSessionsQueueAsync_DeclaresDurableQueue()
    {
        var channel = Substitute.For<IChannel>();
        var publisher = new EventPublisher(channel);

        await publisher.DeclareSessionsQueueAsync();

        await channel.Received(1).QueueDeclareAsync(
            queue: "sessions.available",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishSessionsAsync_PublishesEachSessionToQueue()
    {
        var channel = Substitute.For<IChannel>();
        var publisher = new EventPublisher(channel);
        var sessions = new[]
        {
            new SessionAvailable(
                11731, 1308, 2026, "Race", "Race", "Kuala Lumpur", "Bahrain",
                DateTimeOffset.Parse("2026-10-04T07:00:00+00:00"),
                DateTimeOffset.Parse("2026-10-04T09:00:00+00:00"),
                false),
            new SessionAvailable(
                11730, 1308, 2026, "Qualifying", "Qualifying", "Kuala Lumpur", "Bahrain",
                DateTimeOffset.Parse("2026-10-03T08:00:00+00:00"),
                DateTimeOffset.Parse("2026-10-03T09:00:00+00:00"),
                false)
        };

        await publisher.PublishSessionsAsync(sessions);

        await channel.Received(2).BasicPublishAsync(
            exchange: string.Empty,
            routingKey: "sessions.available",
            mandatory: true,
            basicProperties: Arg.Is<BasicProperties>(p => p.Persistent),
            body: Arg.Any<ReadOnlyMemory<byte>>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
