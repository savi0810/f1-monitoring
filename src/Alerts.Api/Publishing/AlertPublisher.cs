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

    public Task DeclareQueueAsync(CancellationToken cancellationToken = default) =>
        _channel.QueueDeclareAsync(
            queue: QueueNames.AlertTriggered,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

    public async Task PublishAsync(Guid alertId, string payload, CancellationToken cancellationToken = default)
    {
        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueNames.AlertTriggered,
            mandatory: true,
            basicProperties: new BasicProperties { Persistent = true, MessageId = alertId.ToString() },
            body: Encoding.UTF8.GetBytes(payload),
            cancellationToken: cancellationToken);
    }
}
