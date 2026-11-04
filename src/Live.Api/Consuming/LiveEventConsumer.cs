using RabbitMQ.Client;

namespace Live.Api.Consuming;

public class LiveEventConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveEventConsumer> _logger;

    public LiveEventConsumer(IChannel channel, IServiceScopeFactory scopeFactory, ILogger<LiveEventConsumer> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
