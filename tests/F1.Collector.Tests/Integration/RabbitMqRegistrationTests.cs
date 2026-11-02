using System.Net;
using F1.Collector.Extensions;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
using F1.Collector.Tests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RabbitMQ.Client;

namespace F1.Collector.Tests.Integration;

public class RabbitMqRegistrationTests
{
    [Fact]
    public async Task AddRabbitMq_UsesSeparateChannelsForConsumersEventsAndCatalog()
    {
        var connection = Substitute.For<IConnection>();
        var consumerChannel = Substitute.For<IChannel>();
        var eventsChannel = Substitute.For<IChannel>();
        var catalogChannel = Substitute.For<IChannel>();

        connection.CreateChannelAsync(
                Arg.Any<CreateChannelOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(consumerChannel),
                Task.FromResult(eventsChannel),
                Task.FromResult(catalogChannel));

        var services = new ServiceCollection();
        services.AddRabbitMq(new ConfigurationBuilder().Build());
        services.AddSingleton(connection);
        await using var provider = services.BuildServiceProvider();

        var consumer = provider.GetRequiredService<IChannel>();
        var events = provider.GetRequiredService<EventPublisher>();
        var catalog = provider.GetRequiredKeyedService<EventPublisher>("catalog");

        Assert.Same(consumerChannel, consumer);
        Assert.Same(eventsChannel, provider.GetRequiredKeyedService<IChannel>("events"));
        Assert.Same(catalogChannel, provider.GetRequiredKeyedService<IChannel>("catalog"));
        Assert.NotSame(events, catalog);

        await events.DeclareExchangeAsync();
        await catalog.DeclareSessionsQueueAsync();

        await eventsChannel.Received(1).ExchangeDeclareAsync(
            exchange: "f1.events",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: Arg.Any<CancellationToken>());

        await catalogChannel.Received(1).QueueDeclareAsync(
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
    public async Task SessionCatalogPublisher_UsesCatalogChannelFromDi()
    {
        var connection = Substitute.For<IConnection>();
        var eventsChannel = Substitute.For<IChannel>();
        var catalogChannel = Substitute.For<IChannel>();

        connection.CreateChannelAsync(
                Arg.Any<CreateChannelOptions>(),
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(eventsChannel),
                Task.FromResult(catalogChannel));

        using var httpClient = new HttpClient(
            new FakeHttpMessageHandler(HttpStatusCode.OK, "[]"))
        {
            BaseAddress = new Uri("https://api.openf1.org/v1/")
        };
        await using var limiter = new OpenF1RateLimiter(
            requestsPerSecond: 1000, requestsPerMinute: 1000);

        var services = new ServiceCollection();
        services.AddRabbitMq(new ConfigurationBuilder().Build());
        services.AddSingleton(connection);
        services.AddSingleton(new OpenF1Client(httpClient, limiter));
        services.AddLogging();
        services.AddSingleton<SessionCatalogPublisher>();
        await using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<EventPublisher>();
        var publisher = provider.GetRequiredService<SessionCatalogPublisher>();

        await publisher.PublishCatalogAsync();

        await catalogChannel.Received(1).QueueDeclareAsync(
            queue: "sessions.available",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
