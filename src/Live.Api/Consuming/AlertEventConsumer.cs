using System.Text;
using System.Text.Json;
using F1Monitor.Contracts;
using Live.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Live.Api.Consuming;

public class AlertEventConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IHubContext<LiveHub> _hub;
    private readonly ILogger<AlertEventConsumer> _logger;

    public AlertEventConsumer([FromKeyedServices("alerts-consumer")] IChannel channel, IHubContext<LiveHub> hub, ILogger<AlertEventConsumer> logger)
    {
        _channel = channel;
        _hub = hub;
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
            queue: QueueNames.AlertTriggered,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.AlertTriggered,
            exchange: ExchangeNames.F1Live,
            routingKey: RoutingKeys.For(RoutingKeys.Alerts, EventTypes.AlertTriggered),
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += (_, args) => HandleAsync(args, stoppingToken);

        await _channel.BasicConsumeAsync(QueueNames.AlertTriggered, autoAck: false, consumer, stoppingToken);
    }

    private async Task HandleAsync(BasicDeliverEventArgs args, CancellationToken cancellationToken)
    {
        try
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var alert = EventSerializer.Deserialize<AlertTriggered>(json);

            if (alert.AlertId == Guid.Empty || alert.UserId == Guid.Empty)
                throw new JsonException("Invalid alert event.");

            await _hub.Clients.User(alert.UserId.ToString())
                .SendAsync("AlertTriggered", alert, cancellationToken);

            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Invalid alert event.");
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Alert event processing failed.");
            await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, cancellationToken: cancellationToken);
        }
    }
}
