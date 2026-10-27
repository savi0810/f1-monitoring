using F1.Collector.OpenF1;
using F1.Collector.Publishing;
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

    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
}
