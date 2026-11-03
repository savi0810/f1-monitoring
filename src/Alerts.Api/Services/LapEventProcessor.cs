using Alerts.Api.Data;
using F1Monitor.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Services;

public class LapEventProcessor
{
    private readonly AppDbContext _context;
    private readonly LapRuleMatcher _matcher;

    public LapEventProcessor(AppDbContext context, LapRuleMatcher matcher)
    {
        _context = context;
        _matcher = matcher;
    }

    public async Task ProcessAsync(RaceEvent raceEvent, CancellationToken cancellationToken = default)
    {
        if (raceEvent.EventId == Guid.Empty)
            throw new ArgumentException("Event ID is required.", nameof(raceEvent));

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var inserted = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ProcessedEvents" ("EventId", "ProcessedAt")
            VALUES ({raceEvent.EventId}, {DateTimeOffset.UtcNow})
            ON CONFLICT ("EventId") DO NOTHING
            """, cancellationToken);

        if (inserted == 0)
            return;

        var rules = await _matcher.MatchAsync(raceEvent, cancellationToken);

        if (rules.Count > 0)
        {
            var payload = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);

            foreach (var rule in rules)
                _context.OutboxMessages.Add(AlertOutboxFactory.Create(rule, raceEvent.EventId, payload.LapDuration!.Value));
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
