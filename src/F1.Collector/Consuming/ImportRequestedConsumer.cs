using F1.Collector.Mapping;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
using F1Monitor.Contracts;
using RabbitMQ.Client;

namespace F1.Collector.Consuming;

public class ImportRequestedConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly OpenF1Client _openF1Client;
    private readonly EventPublisher _eventPublisher;

    public ImportRequestedConsumer(IChannel channel, OpenF1Client openF1Client, EventPublisher eventPublisher)
    {
        _channel = channel;
        _openF1Client = openF1Client;
        _eventPublisher = eventPublisher;
    }

    public async Task HandleImportRequestedAsync(ImportRequested request, CancellationToken cancellationToken = default)
    {
        var laps = await _openF1Client.GetLapsAsync(request.SessionKey, cancellationToken);
        var events = laps.Select(LapEventMapper.Map);

        await _eventPublisher.PublishBatchAsync(events, cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
