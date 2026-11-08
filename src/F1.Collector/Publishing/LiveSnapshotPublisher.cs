using System.Text.Json;
using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace F1.Collector.Publishing;

public class LiveSnapshotPublisher
{
    private readonly IChannel _channel;

    public LiveSnapshotPublisher(IChannel channel)
    {
        _channel = channel;
    }

    public Task DeclareExchangeAsync(CancellationToken cancellationToken = default)
    {
        return _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Live,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    public async Task PublishAsync(LiveRaceSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = snapshot.EventId.ToString()
        };

        await _channel.BasicPublishAsync(
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.LiveSnapshot),
            mandatory: true,
            basicProperties: properties,
            body: JsonSerializer.SerializeToUtf8Bytes(snapshot),
            cancellationToken: cancellationToken);
    }
}
