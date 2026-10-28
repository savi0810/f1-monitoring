using System.Text;
using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace F1.Collector.Publishing;

public class EventPublisher
{
    private readonly IChannel _channel;

    public EventPublisher(IChannel channel)
    {
        _channel = channel;
    }

    public Task DeclareExchangeAsync(CancellationToken cancellationToken = default) =>
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Events,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

    public async Task PublishToExchangeAsync(
        IEnumerable<RaceEvent> events,
        string source,
        CancellationToken cancellationToken = default)
    {
        var properties = new BasicProperties { Persistent = true };
        var publishTasks = new List<ValueTask>();

        foreach (var raceEvent in events)
        {
            var body = Encoding.UTF8.GetBytes(EventSerializer.Serialize(raceEvent));

            publishTasks.Add(_channel.BasicPublishAsync(
                exchange: ExchangeNames.F1Events,
                routingKey: RoutingKeys.For(source, raceEvent.Type),
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
