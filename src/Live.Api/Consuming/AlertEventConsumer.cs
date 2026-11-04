using System.Text;
using System.Text.Json;
using F1Monitor.Contracts;
using Live.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

    private async Task HandleAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var alert = EventSerializer.Deserialize<AlertTriggered>(json);

            if (alert.AlertId == Guid.Empty || alert.UserId == Guid.Empty)
                throw new JsonException("Invalid alert event.");

            await _hub.Clients.User(alert.UserId.ToString())
                .SendAsync("AlertTriggered", alert, cancellationToken);

            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Invalid alert event.");
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Alert event processing failed.");
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
        }
    }
}
