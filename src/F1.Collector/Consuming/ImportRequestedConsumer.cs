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
    private readonly ILogger<ImportRequestedConsumer> _logger;

    public ImportRequestedConsumer(
        IChannel channel,
        OpenF1Client openF1Client,
        EventPublisher eventPublisher,
        ILogger<ImportRequestedConsumer> logger)
    {
        _channel = channel;
        _openF1Client = openF1Client;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task HandleImportRequestedAsync(ImportRequested request, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.SessionKey);

        var session = await _openF1Client.GetSessionAsync(request.SessionKey, cancellationToken);
        if (session is null)
            throw new KeyNotFoundException(
                $"OpenF1 session {request.SessionKey} was not found.");

        var laps = await _openF1Client.GetLapsAsync(request.SessionKey, cancellationToken);
        var events = laps.Select(lap => LapEventMapper.Map(lap, session));

        await _eventPublisher.PublishToExchangeAsync(events, RoutingKeys.History, cancellationToken);
        await PublishImportFinishedAsync(request, laps.Count, null, cancellationToken);
    }

    private Task PublishImportFinishedAsync(
        ImportRequested request,
        int lapCount,
        string? error,
        CancellationToken cancellationToken)
    {
        var payload = new SessionImportFinishedPayload(request.ImportId, lapCount, error);
        var raceEvent = new RaceEvent(
            EventIdFactory.Create($"{EventTypes.SessionImportFinished}:{request.ImportId}"),
            EventTypes.SessionImportFinished,
            request.SessionKey,
            null,
            null,
            EventSerializer.WritePayload(payload));

        return _eventPublisher.PublishToExchangeAsync([raceEvent], RoutingKeys.History, cancellationToken);
    }

    public async Task<bool> TryHandleAsync(string json, CancellationToken cancellationToken = default)
    {
        ImportRequested request;
        try
        {
            request = EventSerializer.Deserialize<ImportRequested>(json);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Import request failed: {Body}", json);
            return false;
        }

        try
        {
            await HandleImportRequestedAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Import request failed: {Body}", json);

            try
            {
                await PublishImportFinishedAsync(request, 0, ex.Message, cancellationToken);
                return true;
            }
            catch (Exception publishException) when (publishException is not OperationCanceledException)
            {
                _logger.LogError(publishException, "Failed to publish import failure for session {SessionKey}", request.SessionKey);
                return false;
            }
        }
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

            if (await TryHandleAsync(json, stoppingToken))
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            else
                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
        };

        await _channel.BasicConsumeAsync(QueueNames.ImportRequested, autoAck: false, consumer, stoppingToken);
    }
}
