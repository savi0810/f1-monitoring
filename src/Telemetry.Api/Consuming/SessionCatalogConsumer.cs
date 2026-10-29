using RabbitMQ.Client;

namespace Telemetry.Api.Consuming;

public class SessionCatalogConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCatalogConsumer> _logger;

    public SessionCatalogConsumer(
        IChannel channel,
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCatalogConsumer> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
