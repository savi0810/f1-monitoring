using Telemetry.Api.Data;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Tests.Infrastructure;

namespace Telemetry.Api.Tests.Integration;

public class LapRepositoryTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();

    public void Dispose() => _db.Dispose();

    private static Lap NewLap(Guid eventId, int sessionKey = 11396, int lapNumber = 1) => new()
    {
        EventId = eventId,
        SessionKey = sessionKey,
        DriverNumber = 1,
        LapNumber = lapNumber,
        LapDuration = 106.254,
        Sector1Duration = 30.372,
        Sector2Duration = 40.447,
        Sector3Duration = 35.435,
        IsPitOutLap = false,
        OccurredAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task AddLapAsync_NewLap_ReturnsTrue()
    {
        var added = await new LapRepository(_db.Create()).AddLapAsync(NewLap(Guid.NewGuid()));

        Assert.True(added);
    }

    [Fact]
    public async Task AddLapAsync_DuplicateEventId_ReturnsFalse()
    {
        var eventId = Guid.NewGuid();
        await new LapRepository(_db.Create()).AddLapAsync(NewLap(eventId));

        var addedAgain = await new LapRepository(_db.Create()).AddLapAsync(NewLap(eventId));

        Assert.False(addedAgain);
    }
}
