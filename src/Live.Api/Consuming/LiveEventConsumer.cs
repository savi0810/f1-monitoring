using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace Live.Api.Consuming;

public class LiveEventConsumer : BackgroundService
{
    private const string DeadLetterExchange = "live.dead";
    private const string DeadLetterQueue = "live.events.dlq";
    private const string DeadLetterRoutingKey = "live.events.dead";

    private readonly IChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LiveEventConsumer> _logger;

    public LiveEventConsumer(IChannel channel, IServiceScopeFactory scopeFactory, ILogger<LiveEventConsumer> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
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
            queue: QueueNames.LiveEvents,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-routing-key"] = DeadLetterRoutingKey
            },
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.LiveEvents,
            exchange: ExchangeNames.F1Events,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.TimingUpdated),
            cancellationToken: stoppingToken);
    }
}
