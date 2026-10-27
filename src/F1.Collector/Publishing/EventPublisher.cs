using RabbitMQ.Client;

namespace F1.Collector.Publishing;

public class EventPublisher
{
    private readonly IChannel _channel;
    private readonly string _queueName;

    public EventPublisher(IChannel channel, string queueName)
    {
        _channel = channel;
        _queueName = queueName;
    }

    public Task DeclareQueueAsync(CancellationToken cancellationToken = default) =>
        _channel.QueueDeclareAsync(
            queue: _queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
}
