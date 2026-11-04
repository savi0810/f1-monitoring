using Live.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;

namespace Live.Api.Consuming;

public class AlertEventConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IHubContext<LiveHub> _hub;
    private readonly ILogger<AlertEventConsumer> _logger;

    public AlertEventConsumer(IChannel channel, IHubContext<LiveHub> hub, ILogger<AlertEventConsumer> logger)
    {
        _channel = channel;
        _hub = hub;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.CompletedTask;
    }
}
