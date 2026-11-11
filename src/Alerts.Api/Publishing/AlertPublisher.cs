using System.Text;
using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace Alerts.Api.Publishing;

public class AlertPublisher
{
    private readonly IChannel _channel;

    public AlertPublisher(IChannel channel)
    {
        _channel = channel;
    }

    public Task DeclareExchangeAsync(CancellationToken cancellationToken = default) =>
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Live,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

    public async Task DeclareQueueAsync(CancellationToken cancellationToken = default)
    {
        await _channel.QueueDeclareAsync(
            queue: QueueNames.AlertTriggered,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.AlertTriggered,
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Alerts, EventTypes.AlertTriggered),
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(Guid alertId, string payload, CancellationToken cancellationToken = default)
    {
        await _channel.BasicPublishAsync(
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Alerts, EventTypes.AlertTriggered),
            mandatory: true,
            basicProperties: new BasicProperties
            {
                Persistent = true,
                MessageId = alertId.ToString(),
                ContentType = "application/json",
                Type = EventTypes.AlertTriggered
            },
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken);
    }
}
