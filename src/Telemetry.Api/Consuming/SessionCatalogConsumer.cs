using F1Monitor.Contracts;
using RabbitMQ.Client;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Consuming;

public class SessionCatalogConsumer : BackgroundService
{
    private readonly IChannel _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SessionCatalogConsumer> _logger;

    public SessionCatalogConsumer(
        IChannel channel,
        IServiceScopeFactory scopeFactory,
        ILogger<SessionCatalogConsumer> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleSessionAvailableAsync(SessionAvailable available)
    {
        var session = new Session
        {
            SessionKey = available.SessionKey,
            MeetingKey = available.MeetingKey,
            Year = available.Year,
            SessionType = available.SessionType,
            SessionName = available.SessionName,
            CircuitShortName = available.CircuitShortName,
            CountryName = available.CountryName,
            DateStart = available.DateStart,
            DateEnd = available.DateEnd,
            IsCancelled = available.IsCancelled
        };

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        await repository.UpsertSessionAsync(session);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _channel.QueueDeclareAsync(
            queue: QueueNames.SessionsAvailable,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);
    }
}
