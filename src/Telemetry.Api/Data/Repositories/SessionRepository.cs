using Microsoft.EntityFrameworkCore;
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
}
