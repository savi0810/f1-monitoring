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
}
