using System.Text;
using F1Monitor.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Mapping;

namespace Telemetry.Api.Consuming;

public class LapEventConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;

    public LapEventConsumer(IChannel channel, IServiceScopeFactory scopeFactory)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
    }

    public async Task HandleRaceEventAsync(RaceEvent raceEvent, CancellationToken cancellationToken = default)
    {
        if (raceEvent.Type != EventTypes.LapCompleted)
            return;

        var payload = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);
        var lap = LapMapper.FromRaceEvent(raceEvent, payload);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ILapRepository>();

        await repository.AddLapAsync(lap);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _channel.ExchangeDeclareAsync(
            exchange: ExchangeNames.F1Events,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            queue: QueueNames.F1Events,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var raceEvent = EventSerializer.Deserialize(json);

            await HandleRaceEventAsync(raceEvent, stoppingToken);

            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueNames.F1Events, autoAck: false, consumer, stoppingToken);
    }
}
