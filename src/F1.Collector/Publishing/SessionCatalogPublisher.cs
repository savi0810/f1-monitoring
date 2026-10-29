using F1.Collector.Mapping;
using F1.Collector.OpenF1;

namespace F1.Collector.Publishing;

public class SessionCatalogPublisher : BackgroundService
{
    private const int FirstYear = 2023;

    private readonly OpenF1Client _openF1Client;
    private readonly EventPublisher _eventPublisher;
    private readonly ILogger<SessionCatalogPublisher> _logger;

    public SessionCatalogPublisher(
        OpenF1Client openF1Client,
        EventPublisher eventPublisher,
        ILogger<SessionCatalogPublisher> logger)
    {
        _openF1Client = openF1Client;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task PublishCatalogAsync(CancellationToken cancellationToken = default)
    {
        await _eventPublisher.DeclareSessionsQueueAsync(cancellationToken);

        for (var year = FirstYear; year <= DateTime.UtcNow.Year; year++)
        {
            var sessions = await _openF1Client.GetSessionsAsync(year, cancellationToken);

            await _eventPublisher.PublishSessionsAsync(
                sessions.Where(session => !session.IsCancelled).Select(SessionAvailableMapper.Map),
                cancellationToken);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await PublishCatalogAsync(stoppingToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Sessions catalog was not published.");
        }
    }
}
