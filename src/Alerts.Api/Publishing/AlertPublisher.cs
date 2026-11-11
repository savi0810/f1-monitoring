using System.Text;
using System.Text.Json;
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

    public async Task PublishAsync(Guid alertId, string payload, CancellationToken cancellationToken = default)
    {
        var alert = EventSerializer.Deserialize<PositionAlertTriggered>(payload);

        if (alertId == Guid.Empty
            || alert.AlertId != alertId
            || alert.UserId == Guid.Empty
            || alert.SubscriptionId == Guid.Empty
            || alert.EventId == Guid.Empty)
            throw new JsonException("Invalid position alert outbox message.");

        await _channel.BasicPublishAsync(
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Alerts, EventTypes.PositionAlertTriggered),
            mandatory: true,
            basicProperties: new BasicProperties
            {
                Persistent = true,
                MessageId = alertId.ToString(),
                ContentType = "application/json",
                Type = EventTypes.PositionAlertTriggered
            },
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken);
    }
}
