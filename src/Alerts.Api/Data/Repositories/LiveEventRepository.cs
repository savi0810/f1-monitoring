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

    public async Task SaveAsync(Guid eventId, IReadOnlyCollection<OutboxMessage> messages, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var inserted = await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ProcessedEvents" ("EventId", "ProcessedAt")
            VALUES ({eventId}, {DateTimeOffset.UtcNow})
            ON CONFLICT ("EventId") DO NOTHING
            """, cancellationToken);

        if (inserted == 0)
            return;

        _context.OutboxMessages.AddRange(messages);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<bool> SaveCompletionAsync(CompletedRace race, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

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
}
