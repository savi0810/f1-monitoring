using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task UpsertSessionAsync(Session session)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Sessions" ("SessionKey", "MeetingKey", "Year", "SessionType", "SessionName",
                                    "CircuitShortName", "CountryName", "DateStart", "DateEnd", "IsCancelled")
            VALUES ({session.SessionKey}, {session.MeetingKey}, {session.Year}, {session.SessionType}, {session.SessionName},
                    {session.CircuitShortName}, {session.CountryName}, {session.DateStart.ToUniversalTime()}, {session.DateEnd.ToUniversalTime()}, {session.IsCancelled})
            ON CONFLICT ("SessionKey") DO UPDATE SET
                "MeetingKey" = EXCLUDED."MeetingKey",
                "Year" = EXCLUDED."Year",
                "SessionType" = EXCLUDED."SessionType",
                "SessionName" = EXCLUDED."SessionName",
                "CircuitShortName" = EXCLUDED."CircuitShortName",
                "CountryName" = EXCLUDED."CountryName",
                "DateStart" = EXCLUDED."DateStart",
                "DateEnd" = EXCLUDED."DateEnd",
                "IsCancelled" = EXCLUDED."IsCancelled"
            """);
    }

    public async Task<Session?> GetByKeyAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.SessionKey == sessionKey, cancellationToken);
    }

    public async Task<bool> TryBeginImportAsync(
        int sessionKey,
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var updated = await _context.Sessions
            .Where(s => s.SessionKey == sessionKey
                && !s.IsCancelled
                && (s.ImportStatus == null
                    || s.ImportStatus == ImportStatus.Failed))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.ImportId, (Guid?)importId)
                .SetProperty(s => s.ImportStatus, (ImportStatus?)ImportStatus.Pending)
                .SetProperty(s => s.ImportCompletedAt, (DateTimeOffset?)null)
                .SetProperty(s => s.ImportError, (string?)null),
                cancellationToken);

        return updated > 0;
    }

    public async Task<bool> MarkImportFailedAsync(
        int sessionKey,
        Guid importId,
        string error,
        CancellationToken cancellationToken = default)
    {
        var updated = await _context.Sessions
            .Where(s => s.SessionKey == sessionKey
                && s.ImportId == importId
                && s.ImportStatus == ImportStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.ImportStatus, (ImportStatus?)ImportStatus.Failed)
                .SetProperty(s => s.ImportError, error),
                cancellationToken);

        return updated > 0;
    }

    public async Task<bool> MarkImportFinishedAsync(
        int sessionKey,
        Guid importId,
        int lapCount,
        CancellationToken cancellationToken = default)
    {
        var status = lapCount > 0 ? ImportStatus.Completed : ImportStatus.Empty;
        var updated = await _context.Sessions
            .Where(s => s.SessionKey == sessionKey && s.ImportId == importId && s.ImportStatus == ImportStatus.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.ImportStatus, (ImportStatus?)status)
                .SetProperty(s => s.ImportCompletedAt, (DateTimeOffset?)DateTimeOffset.UtcNow)
                .SetProperty(s => s.ImportError, (string?)null),
                cancellationToken);

        return updated > 0;
    }

    public async Task<IReadOnlyList<SessionCatalogYearDto>> GetCatalogAsync(string? type, int? year)
    {
        var query = _context.Sessions.AsNoTracking();

        if (type is not null)
            query = query.Where(s => s.SessionType == type);

        if (year is not null)
            query = query.Where(s => s.Year == year);

        var entries = await query
            .Select(s => new
            {
                s.Meeting.MeetingName,
                Session = new SessionDto(
                    s.SessionKey,
                    s.MeetingKey,
                    s.Year,
                    s.SessionType,
                    s.SessionName,
                    s.CircuitShortName,
                    s.CountryName,
                    s.DateStart,
                    s.DateEnd,
                    s.IsCancelled)
            })
            .ToListAsync();

        return entries
            .GroupBy(e => e.Session.Year)
            .OrderByDescending(yearGroup => yearGroup.Key)
            .Select(yearGroup => new SessionCatalogYearDto(
                yearGroup.Key,
                yearGroup
                    .GroupBy(e => e.Session.MeetingKey)
                    .OrderByDescending(meetingGroup => meetingGroup.Min(e => e.Session.DateStart))
                    .ThenBy(meetingGroup => meetingGroup.Key)
                    .Select(meetingGroup => new SessionCatalogMeetingDto(
                        meetingGroup.Key,
                        meetingGroup.First().MeetingName,
                        meetingGroup
                            .Select(e => e.Session)
                            .OrderBy(s => s.DateStart)
                            .ThenBy(s => s.SessionKey)
                            .ToList()))
                    .ToList()))
            .ToList();
    }
}
