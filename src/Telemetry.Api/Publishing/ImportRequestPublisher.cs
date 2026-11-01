using RabbitMQ.Client;

namespace Telemetry.Api.Publishing;

public class ImportRequestPublisher
{
    private readonly IChannel _channel;

    public ImportRequestPublisher(IChannel channel)
    {
        _channel = channel;
    }
}
