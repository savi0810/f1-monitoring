using System.Text;
using F1Monitor.Contracts;
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

    public Task DeclareExchangeAsync(CancellationToken cancellationToken = default) =>
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Events,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

    public async Task PublishBatchAsync(IEnumerable<RaceEvent> events, CancellationToken cancellationToken = default)
    {
        var properties = new BasicProperties { Persistent = true };
        var publishTasks = new List<ValueTask>();

        foreach (var raceEvent in events)
        {
            var body = Encoding.UTF8.GetBytes(EventSerializer.Serialize(raceEvent));

            publishTasks.Add(_channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _queueName,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken));
        }

        foreach (var publishTask in publishTasks)
        {
            await publishTask;
        }
    }
}
