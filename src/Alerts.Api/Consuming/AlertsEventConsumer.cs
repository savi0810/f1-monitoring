using System.Text;
using System.Text.Json;
using Alerts.Api.Services;
using F1Monitor.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Alerts.Api.Consuming;

public class AlertsEventConsumer : BackgroundService
{
    private const string DeadLetterExchange = "alerts.dead";
    private const string DeadLetterQueue = "alerts.events.dlq";
    private const string DeadLetterRoutingKey = "alerts.events.dead";

    private readonly IChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertsEventConsumer> _logger;

    public AlertsEventConsumer(IChannel channel, IServiceScopeFactory scopeFactory, ILogger<AlertsEventConsumer> logger)
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
            queue: QueueNames.AlertsEvents,
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
            queue: QueueNames.AlertsEvents,
            exchange: ExchangeNames.F1Events,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.LapCompleted),
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                if (args.RoutingKey != RoutingKeys.For(RoutingKeys.Live, EventTypes.LapCompleted))
                {
                    await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                var json = Encoding.UTF8.GetString(args.Body.Span);
                var raceEvent = EventSerializer.Deserialize(json);

                if (raceEvent.EventId == Guid.Empty)
                    throw new JsonException("Event ID is required.");

                using var scope = _scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<LapEventProcessor>();
                await processor.ProcessAsync(raceEvent, stoppingToken);

                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Invalid alerts event.");
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Alerts event processing failed.");
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueNames.AlertsEvents, autoAck: false, consumer, stoppingToken);
    }
}
