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
    private const string DeadLetterQueue = "alerts.live.events.dlq";
    private const string DeadLetterRoutingKey = "alerts.live.events.dead";

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
            exchange: ExchangeNames.F1Live,
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
            queue: QueueNames.AlertsLiveEvents,
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
            queue: QueueNames.AlertsLiveEvents,
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.DriverPositionsChanged),
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.AlertsLiveEvents,
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.RaceCompleted),
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                await ProcessEventAsync(
                    args.RoutingKey, Encoding.UTF8.GetString(args.Body.Span), stoppingToken);

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
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueNames.AlertsLiveEvents, autoAck: false, consumer, stoppingToken);
    }

    private async Task ProcessEventAsync(string routingKey, string json, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        if (routingKey == RoutingKeys.For(RoutingKeys.Live, EventTypes.DriverPositionsChanged))
        {
            var message = EventSerializer.Deserialize<DriverPositionsChanged>(json);
            var service = scope.ServiceProvider.GetRequiredService<IPositionAlertService>();

            await service.ProcessAsync(message, cancellationToken);
            return;
        }

        if (routingKey == RoutingKeys.For(RoutingKeys.Live, EventTypes.RaceCompleted))
        {
            var message = EventSerializer.Deserialize<RaceCompleted>(json);
            var service = scope.ServiceProvider.GetRequiredService<IRaceCompletionService>();

            await service.ProcessAsync(message, cancellationToken);
            return;
        }

        throw new JsonException("Unexpected alerts routing key.");
    }
}
