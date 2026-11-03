using Alerts.Api.Data;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Tests.Integration;

public class AlertRuleRepositoryTests
{
    [Fact]
    public async Task DeleteAsync_OtherUsersRule_DoesNotDelete()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var rule = new AlertRule
        {
            UserId = Guid.NewGuid(),
            DriverNumber = 1,
            LapTimeThresholdSeconds = 90
        };

        context.AlertRules.Add(rule);
        await context.SaveChangesAsync();

        var deleted = await new AlertRuleRepository(context).DeleteAsync(rule.Id, Guid.NewGuid());

        Assert.False(deleted);
        Assert.True(await context.AlertRules.AnyAsync(item => item.Id == rule.Id));
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyUsersRules()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var userId = Guid.NewGuid();
        var ownRule = new AlertRule { UserId = userId, DriverNumber = 1, LapTimeThresholdSeconds = 90 };
        var otherRule = new AlertRule { UserId = Guid.NewGuid(), DriverNumber = 2, LapTimeThresholdSeconds = 95 };

        context.AlertRules.AddRange(ownRule, otherRule);
        await context.SaveChangesAsync();

        var rules = await new AlertRuleRepository(context).GetByUserIdAsync(userId);

        var rule = Assert.Single(rules);
        Assert.Equal(ownRule.Id, rule.Id);
    }
}
