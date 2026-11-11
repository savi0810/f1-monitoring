using Alerts.Api.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Data.Repositories;

public class LiveEventRepository : ILiveEventRepository
{
    private readonly AppDbContext _context;

    public LiveEventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(Guid eventId, string sessionId, IReadOnlyCollection<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await LockRaceAsync(sessionId, cancellationToken);

        var inserted = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ProcessedEvents" ("EventId", "ProcessedAt")
            VALUES ({eventId}, {DateTimeOffset.UtcNow})
            ON CONFLICT ("EventId") DO NOTHING
            """, cancellationToken);

        if (inserted == 0)
            return;

        var completed = await _context.CompletedRaces
            .AnyAsync(race => race.SessionId == sessionId, cancellationToken);

        if (!completed)
        {
            _context.OutboxMessages.AddRange(messages);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<bool> SaveCompletionAsync(CompletedRace race, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        await LockRaceAsync(race.SessionId, cancellationToken);

        var processed = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ProcessedEvents" ("EventId", "ProcessedAt")
            VALUES ({race.EventId}, {DateTimeOffset.UtcNow})
            ON CONFLICT ("EventId") DO NOTHING
            """, cancellationToken);

        if (processed == 0)
            return false;

        var completed = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "CompletedRaces" ("SessionId", "EventId", "ObservedAt", "RecordedAt")
            VALUES ({race.SessionId}, {race.EventId}, {race.ObservedAt}, {race.RecordedAt})
            ON CONFLICT ("SessionId") DO NOTHING
            """, cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return completed > 0;
    }

    private async Task LockRaceAsync(string sessionId, CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({sessionId}, 0))",
            cancellationToken);
    }
}
