using Alerts.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Publishing;

public class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AlertPublisher _publisher;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, AlertPublisher publisher, ILogger<OutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _publisher.DeclareQueueAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var messages = await context.OutboxMessages
                    .Where(message => message.PublishedAt == null)
                    .OrderBy(message => message.CreatedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    await _publisher.PublishAsync(message.Id, message.Payload, stoppingToken);
                    message.PublishedAt = DateTimeOffset.UtcNow;
                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Outbox dispatch failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
