using Microsoft.EntityFrameworkCore;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Data.Repositories;

public class MeetingRepository : IMeetingRepository
{
    private readonly AppDbContext _context;

    public MeetingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task UpsertMeetingAsync(Meeting meeting)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "Meetings" ("MeetingKey", "MeetingName")
            VALUES ({meeting.MeetingKey}, {meeting.MeetingName})
            ON CONFLICT ("MeetingKey") DO UPDATE SET
                "MeetingName" = COALESCE(EXCLUDED."MeetingName", "Meetings"."MeetingName")
            """);
    }
}
