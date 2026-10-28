using F1Monitor.Contracts;
using RabbitMQ.Client;
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
