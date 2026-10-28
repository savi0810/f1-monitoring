using System.Text;
using F1.Collector.Mapping;
using F1.Collector.OpenF1;
using F1.Collector.Publishing;
using F1Monitor.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
        var session = await _openF1Client.GetSessionAsync(request.SessionKey, cancellationToken);
        if (session is null)
            return;

        var laps = await _openF1Client.GetLapsAsync(request.SessionKey, cancellationToken);
        var events = laps.Select(lap => LapEventMapper.Map(lap, session));

        await _eventPublisher.PublishToExchangeAsync(events, RoutingKeys.History, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _channel.QueueDeclareAsync(
            queue: QueueNames.ImportRequested,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            var json = Encoding.UTF8.GetString(args.Body.Span);
            var request = EventSerializer.Deserialize<ImportRequested>(json);

            await HandleImportRequestedAsync(request, stoppingToken);

            await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueNames.ImportRequested, autoAck: false, consumer, stoppingToken);
    }
}
