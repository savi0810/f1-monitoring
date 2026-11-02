using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace Alerts.Api.Consuming;

public class AlertsEventConsumer : BackgroundService
{
    private const string DeadLetterExchange = "alerts.dead";
    private const string DeadLetterQueue = "alerts.events.dlq";
    private const string DeadLetterRoutingKey = "alerts.events.dead";

    private readonly IChannel _channel;

    public AlertsEventConsumer(IChannel channel)
    {
        _channel = channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Events,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: DeadLetterQueue,
            exchange: DeadLetterExchange,
            routingKey: DeadLetterRoutingKey,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueNames.AlertsEvents,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.AlertsEvents,
            exchange: ExchangeNames.F1Events,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.LapCompleted),
            cancellationToken: stoppingToken);
    }
}
