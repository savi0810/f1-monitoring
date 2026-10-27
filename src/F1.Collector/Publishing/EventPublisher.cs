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
}
