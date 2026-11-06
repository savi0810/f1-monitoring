using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public class SessionDriverRepository : ISessionDriverRepository
{
    private readonly AppDbContext _context;

    public SessionDriverRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(SessionDriver driver)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "SessionDrivers" ("SessionKey", "DriverNumber", "FullName", "TeamName",
                "Position", "NumberOfLaps", "DurationJson", "GapToLeaderJson",
                "DidNotFinish", "DidNotStart", "Disqualified")
            VALUES ({driver.SessionKey}, {driver.DriverNumber}, {driver.FullName}, {driver.TeamName},
                {driver.Position}, {driver.NumberOfLaps}, {driver.DurationJson}, {driver.GapToLeaderJson},
                {driver.DidNotFinish}, {driver.DidNotStart}, {driver.Disqualified})
            ON CONFLICT ("SessionKey", "DriverNumber") DO UPDATE SET
                "FullName" = COALESCE(EXCLUDED."FullName", "SessionDrivers"."FullName"),
                "TeamName" = COALESCE(EXCLUDED."TeamName", "SessionDrivers"."TeamName"),
                "Position" = COALESCE(EXCLUDED."Position", "SessionDrivers"."Position"),
                "NumberOfLaps" = COALESCE(EXCLUDED."NumberOfLaps", "SessionDrivers"."NumberOfLaps"),
                "DurationJson" = COALESCE(EXCLUDED."DurationJson", "SessionDrivers"."DurationJson"),
                "GapToLeaderJson" = COALESCE(EXCLUDED."GapToLeaderJson", "SessionDrivers"."GapToLeaderJson"),
                "DidNotFinish" = COALESCE(EXCLUDED."DidNotFinish", "SessionDrivers"."DidNotFinish"),
                "DidNotStart" = COALESCE(EXCLUDED."DidNotStart", "SessionDrivers"."DidNotStart"),
                "Disqualified" = COALESCE(EXCLUDED."Disqualified", "SessionDrivers"."Disqualified")
            """);
    }
}
