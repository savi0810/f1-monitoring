using Telemetry.Api.Data;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Tests.Infrastructure;

namespace Telemetry.Api.Tests.Integration;

public class SessionRepositoryTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();

    public void Dispose() => _db.Dispose();

    private static Session NewSession(int sessionKey = 11731, string sessionName = "Race") => new()
    {
        SessionKey = sessionKey,
        MeetingKey = 1308,
        Year = 2026,
        SessionType = "Race",
        SessionName = sessionName,
        CircuitShortName = "Kuala Lumpur",
        CountryName = "Bahrain",
        DateStart = new DateTimeOffset(2026, 10, 4, 7, 0, 0, TimeSpan.Zero),
        DateEnd = new DateTimeOffset(2026, 10, 4, 9, 0, 0, TimeSpan.Zero),
        IsCancelled = false
    };

    [Fact]
    public async Task UpsertSessionAsync_NewSession_IsStored()
    {
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession());

        var stored = Assert.Single(_db.Create().Sessions);
        Assert.Equal(11731, stored.SessionKey);
        Assert.Equal("Race", stored.SessionType);
        Assert.Equal("Kuala Lumpur", stored.CircuitShortName);
    }

    [Fact]
    public async Task UpsertSessionAsync_ExistingSession_UpdatesItWithoutDuplicate()
    {
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(sessionName: "Race"));

        var changed = NewSession(sessionName: "Sprint");
        changed.IsCancelled = true;
        await new SessionRepository(_db.Create()).UpsertSessionAsync(changed);

        var stored = Assert.Single(_db.Create().Sessions);
        Assert.Equal("Sprint", stored.SessionName);
        Assert.True(stored.IsCancelled);
    }

    [Fact]
    public async Task UpsertSessionAsync_SessionWithoutCircuitAndCountry_IsStored()
    {
        var session = NewSession();
        session.CircuitShortName = null;
        session.CountryName = null;

        await new SessionRepository(_db.Create()).UpsertSessionAsync(session);

        var stored = Assert.Single(_db.Create().Sessions);
        Assert.Null(stored.CircuitShortName);
        Assert.Null(stored.CountryName);
    }

    [Fact]
    public async Task GetByKeyAsync_ExistingSession_ReturnsSessionWithoutTracking()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        await repository.UpsertSessionAsync(NewSession(11730, "Qualifying"));
        await repository.UpsertSessionAsync(NewSession(11731));

        var session = await repository.GetByKeyAsync(11731);

        Assert.NotNull(session);
        Assert.Equal(11731, session.SessionKey);
        Assert.Equal(1308, session.MeetingKey);
        Assert.Equal(2026, session.Year);
        Assert.Equal("Race", session.SessionType);
        Assert.Equal("Race", session.SessionName);
        Assert.Equal("Kuala Lumpur", session.CircuitShortName);
        Assert.Equal("Bahrain", session.CountryName);
        Assert.Equal(NewSession().DateStart, session.DateStart);
        Assert.Equal(NewSession().DateEnd, session.DateEnd);
        Assert.False(session.IsCancelled);
        Assert.Empty(context.ChangeTracker.Entries<Session>());
    }

    [Fact]
    public async Task GetByKeyAsync_UnknownSession_ReturnsNull()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        await repository.UpsertSessionAsync(NewSession());

        var session = await repository.GetByKeyAsync(1);

        Assert.Null(session);
    }

    private static Lap NewLap(int sessionKey, int lapNumber) => new()
    {
        EventId = Guid.NewGuid(),
        SessionKey = sessionKey,
        DriverNumber = 1,
        LapNumber = lapNumber,
        OccurredAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task GetSessionsAsync_ReturnsSessionsWithLapCount()
    {
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11730));
        var laps = new LapRepository(_db.Create());
        await laps.AddLapAsync(NewLap(11731, 1));
        await laps.AddLapAsync(NewLap(11731, 2));

        var sessions = await new SessionRepository(_db.Create()).GetSessionsAsync(null, null);

        Assert.Equal(2, sessions.Count);
        Assert.Equal(2, sessions.Single(s => s.SessionKey == 11731).LapCount);
        Assert.Equal(0, sessions.Single(s => s.SessionKey == 11730).LapCount);
    }

    [Fact]
    public async Task GetSessionsAsync_FiltersByType()
    {
        var qualifying = NewSession(11730);
        qualifying.SessionType = "Qualifying";
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));
        await new SessionRepository(_db.Create()).UpsertSessionAsync(qualifying);

        var sessions = await new SessionRepository(_db.Create()).GetSessionsAsync("Qualifying", null);

        var session = Assert.Single(sessions);
        Assert.Equal(11730, session.SessionKey);
    }

    [Fact]
    public async Task GetSessionsAsync_FiltersByYear()
    {
        var older = NewSession(9000);
        older.Year = 2025;
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));
        await new SessionRepository(_db.Create()).UpsertSessionAsync(older);

        var sessions = await new SessionRepository(_db.Create()).GetSessionsAsync(null, 2025);

        var session = Assert.Single(sessions);
        Assert.Equal(9000, session.SessionKey);
    }

    [Fact]
    public async Task GetSessionsAsync_ReturnsNewestFirst()
    {
        var older = NewSession(9000);
        older.DateStart = new DateTimeOffset(2026, 3, 1, 7, 0, 0, TimeSpan.Zero);
        older.DateEnd = new DateTimeOffset(2026, 3, 1, 9, 0, 0, TimeSpan.Zero);
        await new SessionRepository(_db.Create()).UpsertSessionAsync(older);
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));

        var sessions = await new SessionRepository(_db.Create()).GetSessionsAsync(null, null);

        Assert.Equal([11731, 9000], sessions.Select(s => s.SessionKey));
    }
}
