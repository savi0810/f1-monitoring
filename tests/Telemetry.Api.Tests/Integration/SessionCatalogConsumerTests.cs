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
            11731, 1308, 2026, "Race", "Race", "Kuala Lumpur", "Bahrain",
            new DateTimeOffset(2026, 10, 4, 7, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 10, 4, 9, 0, 0, TimeSpan.Zero),
            false);

        await CreateConsumer().HandleSessionAvailableAsync(available);

        var stored = Assert.Single(_db.Create().Sessions);
        Assert.Equal(11731, stored.SessionKey);
        Assert.Equal("Race", stored.SessionType);
        Assert.Equal("Kuala Lumpur", stored.CircuitShortName);
    }
}
