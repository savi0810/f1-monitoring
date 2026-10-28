using Microsoft.EntityFrameworkCore;
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

    [Fact]
    public async Task AddLapAsync_DbUpdateFailsWithoutDuplicate_Rethrows()
    {
        await using var context = new FailingSaveContext(_db.Options);

        await Assert.ThrowsAsync<DbUpdateException>(() => new LapRepository(context).AddLapAsync(NewLap(Guid.NewGuid())));
    }

    private sealed class FailingSaveContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new DbUpdateException("save failed");
    }

    [Fact]
    public async Task GetSessionsAsync_GroupsLapsBySessionKey()
    {
        var repository = new LapRepository(_db.Create());
        await repository.AddLapAsync(NewLap(Guid.NewGuid(), sessionKey: 11396, lapNumber: 1));
        await repository.AddLapAsync(NewLap(Guid.NewGuid(), sessionKey: 11396, lapNumber: 2));
        await repository.AddLapAsync(NewLap(Guid.NewGuid(), sessionKey: 9999, lapNumber: 1));

        var races = await new LapRepository(_db.Create()).GetSessionsAsync();

        var race = Assert.Single(races, r => r.SessionKey == 11396);
        Assert.Equal(2, race.LapCount);
    }

    [Fact]
    public async Task GetLapsBySessionKeyAsync_ReturnsOnlyMatchingLaps()
    {
        var repository = new LapRepository(_db.Create());
        await repository.AddLapAsync(NewLap(Guid.NewGuid(), sessionKey: 11396, lapNumber: 1));
        await repository.AddLapAsync(NewLap(Guid.NewGuid(), sessionKey: 9999, lapNumber: 1));

        var laps = await new LapRepository(_db.Create()).GetLapsBySessionKeyAsync(11396);

        Assert.Single(laps);
        Assert.Equal(11396, laps[0].SessionKey);
    }
}
