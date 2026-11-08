using System.Text;
using F1Monitor.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Telemetry.Api.Data.Models;
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
        if (raceEvent.Type != EventTypes.LapCompleted &&
            raceEvent.Type != EventTypes.SessionDriver &&
            raceEvent.Type != EventTypes.SessionImportFinished)
            return;

        using var scope = _scopeFactory.CreateScope();

        if (raceEvent.Type == EventTypes.SessionDriver)
        {
            var driverNumber = raceEvent.DriverNumber
                ?? throw new InvalidOperationException("Session driver event has no driver number.");
            var payload = EventSerializer.ReadPayload<SessionDriverPayload>(raceEvent);
            var driver = new SessionDriver
            {
                SessionKey = raceEvent.SessionKey,
                DriverNumber = driverNumber,
                FullName = payload.FullName,
                TeamName = payload.TeamName,
                Position = payload.Position,
                NumberOfLaps = payload.NumberOfLaps,
                DurationJson = payload.DurationJson,
                GapToLeaderJson = payload.GapToLeaderJson,
                DidNotFinish = payload.DidNotFinish,
                DidNotStart = payload.DidNotStart,
                Disqualified = payload.Disqualified
            };
            var driverRepository = scope.ServiceProvider.GetRequiredService<ISessionDriverRepository>();

            await driverRepository.UpsertAsync(driver);
            return;
        }

        if (raceEvent.Type == EventTypes.LapCompleted)
        {
            var payload = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);
            var lap = LapMapper.FromRaceEvent(raceEvent, payload);
            var lapRepository = scope.ServiceProvider.GetRequiredService<ILapRepository>();

            await lapRepository.AddLapAsync(lap);
            return;
        }

        var finishedPayload = EventSerializer.ReadPayload<SessionImportFinishedPayload>(raceEvent);
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        if (finishedPayload.Error is null)
        {
            await sessionRepository.MarkImportFinishedAsync(
                raceEvent.SessionKey, finishedPayload.ImportId, finishedPayload.LapCount, cancellationToken);
        }
        else
        {
            await sessionRepository.MarkImportFailedAsync(
                raceEvent.SessionKey, finishedPayload.ImportId, finishedPayload.Error, cancellationToken);
        }
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
            queue: QueueNames.TelemetryEvents,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            queue: QueueNames.TelemetryEvents,
            exchange: ExchangeNames.F1Events,
            routingKey: RoutingKeys.AllEvents,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var raceEvent = EventSerializer.Deserialize(json);

            await HandleRaceEventAsync(raceEvent, stoppingToken);

            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueNames.TelemetryEvents, autoAck: false, consumer, stoppingToken);
    }
}
