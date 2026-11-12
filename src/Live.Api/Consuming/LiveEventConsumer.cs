using System.Text.Json;
using F1Monitor.Contracts;
using Live.Api.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Live.Api.Consuming;

public class LiveEventConsumer : BackgroundService
{
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
            exchange: ExchangeNames.F1Live,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueNames.LiveSnapshots,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.LiveSnapshots,
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Live, EventTypes.LiveSnapshot),
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var snapshot = JsonSerializer.Deserialize<LiveRaceSnapshot>(args.Body.Span)
                    ?? throw new JsonException("Snapshot is empty.");

                if (snapshot.EventId == Guid.Empty
                    || string.IsNullOrWhiteSpace(snapshot.Source)
                    || string.IsNullOrWhiteSpace(snapshot.SessionId)
                    || snapshot.ObservedAt == default
                    || snapshot.ObservedAt > DateTimeOffset.UtcNow.AddSeconds(30)
                    || snapshot.Drivers == null)
                    throw new JsonException("Invalid snapshot.");

                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<TimingService>();
                await service.ProcessAsync(snapshot, stoppingToken);

                await _channel.BasicAckAsync(
                    args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Invalid live snapshot.");
                await _channel.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Live snapshot processing failed.");
                await _channel.BasicNackAsync(
                    args.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            QueueNames.LiveSnapshots,
            autoAck: false,
            consumer,
            stoppingToken);
    }
}
