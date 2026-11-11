using System.Text;
using F1Monitor.Contracts;
using Live.Api.Data.DTOs;
using RabbitMQ.Client;

namespace Live.Api.Publishing;

public class LiveOutboxPublisher
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IChannel _channel;
    private readonly ILogger<LiveOutboxPublisher> _logger;

    public LiveOutboxPublisher(IServiceScopeFactory scopeFactory, [FromKeyedServices("live-events-publisher")] IChannel channel, ILogger<LiveOutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _channel = channel;
        _logger = logger;
    }

    public Task DeclareExchangeAsync(CancellationToken cancellationToken = default) =>
        _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Live,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

    public async Task PublishAsync(PendingLiveEventDto pendingEvent, CancellationToken cancellationToken = default)
    {
        var properties = new BasicProperties
        {
            Persistent = true,
            MessageId = pendingEvent.EventId.ToString(),
            ContentType = "application/json",
            Type = pendingEvent.RoutingKey.Split('.')[^1]
        };

        await _channel.BasicPublishAsync(
            exchange: ExchangeNames.F1Live,
            routingKey: pendingEvent.RoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(pendingEvent.Payload),
            cancellationToken: cancellationToken);
    }
}
