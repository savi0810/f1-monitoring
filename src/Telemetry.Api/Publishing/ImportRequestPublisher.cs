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
}
