using System.Text;
using F1Monitor.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Telemetry.Api.Data;
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

        var meeting = new Meeting
        {
            MeetingKey = available.MeetingKey,
            MeetingName = available.MeetingName,
        };

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var meetingRepository = scope.ServiceProvider.GetRequiredService<IMeetingRepository>();
        var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();

        await using var transaction = await context.Database.BeginTransactionAsync();

        await meetingRepository.UpsertMeetingAsync(meeting);
        await sessionRepository.UpsertSessionAsync(session);

        await transaction.CommitAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _channel.QueueDeclareAsync(
            queue: QueueNames.SessionsAvailable,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (_, args) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(args.Body.Span);
                var available = EventSerializer.Deserialize<SessionAvailable>(json);

                await HandleSessionAvailableAsync(available);

                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Session catalog message failed.");

                await _channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(QueueNames.SessionsAvailable, autoAck: false, consumer, stoppingToken);
    }
}
