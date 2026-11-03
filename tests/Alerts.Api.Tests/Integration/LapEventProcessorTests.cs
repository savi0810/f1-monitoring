using Alerts.Api.Data;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;
using Alerts.Api.Services;
using F1Monitor.Contracts;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Tests.Integration;

public class LapEventProcessorTests
{
    [Fact]
    public async Task ProcessAsync_DuplicateEvent_CreatesOneOutboxMessage()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        context.AlertRules.Add(new AlertRule
        {
            UserId = Guid.NewGuid(),
            DriverNumber = 1,
            LapTimeThresholdSeconds = 90
        });
        await context.SaveChangesAsync();

        var raceEvent = new RaceEvent(
            Guid.NewGuid(),
            EventTypes.LapCompleted,
            123,
            1,
            DateTimeOffset.UtcNow,
            EventSerializer.WritePayload(new LapCompletedPayload(1, 85, null, null, null, false)));

        var matcher = new LapRuleMatcher(new AlertRuleRepository(context));
        var processor = new LapEventProcessor(context, matcher);

        await processor.ProcessAsync(raceEvent);
        await processor.ProcessAsync(raceEvent);

        Assert.Single(await context.OutboxMessages.ToListAsync());
    }
}
