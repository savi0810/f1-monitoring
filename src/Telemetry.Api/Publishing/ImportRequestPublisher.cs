using System.Text;
using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace Telemetry.Api.Publishing;

public class ImportRequestPublisher
{
    private readonly IChannel _channel;

    public ImportRequestPublisher(IChannel channel)
    {
        _channel = channel;
    }

    public Task DeclareQueueAsync(CancellationToken cancellationToken = default) =>
        _channel.QueueDeclareAsync(
            queue: QueueNames.ImportRequested,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

    public async Task PublishAsync(
        ImportRequested request,
        CancellationToken cancellationToken = default)
    {
        var body = Encoding.UTF8.GetBytes(EventSerializer.Serialize(request));

        await _channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueNames.ImportRequested,
            mandatory: true,
            basicProperties: new BasicProperties { Persistent = true },
            body: body,
            cancellationToken: cancellationToken);
    }
}
