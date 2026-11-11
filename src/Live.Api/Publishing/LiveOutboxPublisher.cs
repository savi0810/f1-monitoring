using RabbitMQ.Client;

namespace Live.Api.Publishing;

public class LiveOutboxPublisher
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
}
