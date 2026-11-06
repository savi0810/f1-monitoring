using F1Monitor.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using RabbitMQ.Client;
using Telemetry.Api.Consuming;
using Telemetry.Api.Data;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Tests.Infrastructure;

namespace Telemetry.Api.Tests.Integration;

public class SessionCatalogConsumerTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();
    private readonly ServiceProvider _provider;

    public SessionCatalogConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => _db.Create());
        services.AddScoped<ISessionRepository, SessionRepository>();
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _db.Dispose();
    }

    private SessionCatalogConsumer CreateConsumer() =>
        new(
            Substitute.For<IChannel>(),
            _provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<SessionCatalogConsumer>.Instance);

    [Fact]
    public async Task HandleSessionAvailableAsync_StoresSession()
    {
        var available = new SessionAvailable(
            11731, 1308, "Bahrain Grand Prix", 2026, "Race", "Race", "Kuala Lumpur", "Bahrain",
            new DateTimeOffset(2026, 10, 4, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 9, 0, 0, TimeSpan.Zero),
            false);

        await CreateConsumer().HandleSessionAvailableAsync(available);

        var stored = Assert.Single(_db.Create().Sessions);
        Assert.Equal(11731, stored.SessionKey);
        Assert.Equal("Race", stored.SessionType);
        Assert.Equal("Kuala Lumpur", stored.CircuitShortName);
    }

    [Fact]
    public async Task StartAsync_DeclaresSessionsQueue()
    {
        var channel = Substitute.For<IChannel>();
        var consumer = new SessionCatalogConsumer(
            channel,
            _provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<SessionCatalogConsumer>.Instance);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.ExecuteTask!;

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
    public async Task StartAsync_ConsumesSessionsQueue()
    {
        var channel = Substitute.For<IChannel>();
        var consumer = new SessionCatalogConsumer(
            channel,
            _provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<SessionCatalogConsumer>.Instance);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.ExecuteTask!;

        await channel.Received(1).BasicConsumeAsync(
            queue: "sessions.available",
            autoAck: false,
            consumerTag: Arg.Any<string>(),
            noLocal: false,
            exclusive: false,
            arguments: null,
            consumer: Arg.Any<IAsyncBasicConsumer>(),
            cancellationToken: Arg.Any<CancellationToken>());
    }
}
