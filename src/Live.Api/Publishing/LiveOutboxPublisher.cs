using System.Text;
using F1Monitor.Contracts;
using Live.Api.Data.DTOs;
using Live.Api.Data.Repositories;
using RabbitMQ.Client;

namespace Live.Api.Publishing;

public class LiveOutboxPublisher : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DeclareExchangeAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<ILiveSnapshotRepository>();
                var pendingEvent = await repository.GetPendingEventAsync(stoppingToken);

                if (pendingEvent == null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                await PublishAsync(pendingEvent, stoppingToken);
                await repository.RemovePendingEventAsync(pendingEvent.EventId, stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Live outbox publishing failed.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
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
