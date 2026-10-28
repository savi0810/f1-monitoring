using F1Monitor.Contracts;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RabbitMQ.Client;
using Telemetry.Api.Consuming;
using Telemetry.Api.Data;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Tests.Infrastructure;

namespace Telemetry.Api.Tests.Integration;

public class LapEventConsumerTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();
    private readonly ServiceProvider _provider;

    public LapEventConsumerTests()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => _db.Create());
        services.AddScoped<ILapRepository, LapRepository>();
        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        _db.Dispose();
    }

    private LapEventConsumer CreateConsumer() =>
        new(Substitute.For<IChannel>(), _provider.GetRequiredService<IServiceScopeFactory>());

    [Fact]
    public async Task HandleRaceEventAsync_LapCompleted_PersistsLap()
    {
        var raceEvent = new RaceEvent(Guid.NewGuid(), EventTypes.LapCompleted, 11396, 1, DateTimeOffset.UtcNow,
            EventSerializer.WritePayload(new LapCompletedPayload(1, 100, 30, 40, 30, false)));

        await CreateConsumer().HandleRaceEventAsync(raceEvent);

        var repository = new LapRepository(_db.Create());
        Assert.Single(await repository.GetLapsBySessionKeyAsync(11396));
    }

    [Fact]
    public async Task HandleRaceEventAsync_UnknownType_DoesNothing()
    {
        var raceEvent = new RaceEvent(Guid.NewGuid(), "pit_stop", 11396, 1, DateTimeOffset.UtcNow,
            EventSerializer.WritePayload(new LapCompletedPayload(1, 100, 30, 40, 30, false)));

        await CreateConsumer().HandleRaceEventAsync(raceEvent);

        var repository = new LapRepository(_db.Create());
        Assert.Empty(await repository.GetSessionsAsync());
    }
}
