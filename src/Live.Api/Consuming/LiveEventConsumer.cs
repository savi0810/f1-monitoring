using System.Text;
using System.Text.Json;
using F1Monitor.Contracts;
using Live.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

        await _channel.QueueBindAsync(
            queue: QueueNames.LiveEvents,
            exchange: ExchangeNames.F1Events,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.SourceStatus),
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);
                using var scope = _scopeFactory.CreateScope();

                if (args.RoutingKey == RoutingKeys.For(RoutingKeys.Live, EventTypes.SourceStatus))
                {
                    var status = EventSerializer.Deserialize<LiveSourceStatus>(json);
                    if (status.ReportedAt == default || status.ReportedAt > DateTimeOffset.UtcNow.AddSeconds(30) || status.SessionKey is <= 0)
                        throw new JsonException("Invalid source status.");

                    var service = scope.ServiceProvider.GetRequiredService<LiveSourceService>();
                    await service.ProcessAsync(status, stoppingToken);
                }
                else
                {
                    var raceEvent = EventSerializer.Deserialize(json);
                    if (raceEvent.EventId == Guid.Empty || raceEvent.Type != EventTypes.TimingUpdated)
                        throw new JsonException("Invalid timing event.");

                    var service = scope.ServiceProvider.GetRequiredService<TimingService>();
                    await service.ProcessAsync(raceEvent, stoppingToken);
                }

                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Invalid timing event.");
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Timing event processing failed.");
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueNames.LiveEvents, autoAck: false, consumer, stoppingToken);
    }
}
