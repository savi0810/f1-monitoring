using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace Alerts.Api.Consuming;

public class AlertsEventConsumer : BackgroundService
{
    private readonly IChannel _channel;

    public AlertsEventConsumer(IChannel channel)
    {
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Events,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueNames.AlertsEvents,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.AlertsEvents,
            exchange: ExchangeNames.F1Events,
            routingKey: RoutingKeys.LiveEvents,
            cancellationToken: stoppingToken);
    }
}
