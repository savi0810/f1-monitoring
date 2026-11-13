using Telemetry.Api.Data;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Tests.Infrastructure;

namespace Telemetry.Api.Tests.Integration;

public class SessionRepositoryTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();

    public SessionRepositoryTests()
    {
        using var context = _db.Create();
        context.Meetings.Add(new Meeting { MeetingKey = 1308, MeetingName = "Bahrain Grand Prix" });
        context.SaveChanges();
    }

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

    [Fact]
    public async Task GetByKeyAsync_CancelledSession_ReturnsSession()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        var cancelled = NewSession();
        cancelled.IsCancelled = true;
        await repository.UpsertSessionAsync(cancelled);

        var session = await repository.GetByKeyAsync(cancelled.SessionKey);

        Assert.NotNull(session);
        Assert.True(session.IsCancelled);
    }

    [Fact]
    public async Task GetByKeyAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repository.GetByKeyAsync(11731, cancellation.Token));
    }

    [Fact]
    public async Task TryBeginImportAsync_SecondRequest_DoesNotReplacePendingImport()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        await repository.UpsertSessionAsync(NewSession());
        var firstImportId = Guid.NewGuid();

        var first = await repository.TryBeginImportAsync(11731, firstImportId);
        var second = await repository.TryBeginImportAsync(11731, Guid.NewGuid());

        Assert.True(first);
        Assert.False(second);

        var session = await repository.GetByKeyAsync(11731);
        Assert.NotNull(session);
        Assert.Equal(firstImportId, session.ImportId);
        Assert.Equal(ImportStatus.Pending, session.ImportStatus);
    }

    [Fact]
    public async Task MarkImportFailedAsync_MatchingImportId_MarksPendingImportAsFailed()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        await repository.UpsertSessionAsync(NewSession());
        var importId = Guid.NewGuid();

        await repository.TryBeginImportAsync(11731, importId);

        var wrongImportUpdated = await repository.MarkImportFailedAsync(
            11731,
            Guid.NewGuid(),
            "Publish failed");

        var matchingImportUpdated = await repository.MarkImportFailedAsync(
            11731,
            importId,
            "Publish failed");

        var session = await repository.GetByKeyAsync(11731);

        Assert.False(wrongImportUpdated);
        Assert.True(matchingImportUpdated);
        Assert.NotNull(session);
        Assert.Equal(ImportStatus.Failed, session.ImportStatus);
        Assert.Equal("Publish failed", session.ImportError);
    }

    [Fact]
    public async Task MarkImportFinishedAsync_MatchingImportId_CompletesPendingImport()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        await repository.UpsertSessionAsync(NewSession());
        var importId = Guid.NewGuid();

        await repository.TryBeginImportAsync(11731, importId);

        var wrongImportUpdated = await repository.MarkImportFinishedAsync(11731, Guid.NewGuid(), 2);
        var matchingImportUpdated = await repository.MarkImportFinishedAsync(11731, importId, 2);
        var session = await repository.GetByKeyAsync(11731);

        Assert.False(wrongImportUpdated);
        Assert.True(matchingImportUpdated);
        Assert.NotNull(session);
        Assert.Equal(ImportStatus.Completed, session.ImportStatus);
        Assert.NotNull(session.ImportCompletedAt);
        Assert.Null(session.ImportError);
    }

    [Fact]
    public async Task MarkImportFinishedAsync_NoLaps_MarksImportAsEmpty()
    {
        await using var context = _db.Create();
        var repository = new SessionRepository(context);
        await repository.UpsertSessionAsync(NewSession());
        var importId = Guid.NewGuid();

        await repository.TryBeginImportAsync(11731, importId);

        var updated = await repository.MarkImportFinishedAsync(11731, importId, 0);
        var session = await repository.GetByKeyAsync(11731);

        Assert.True(updated);
        Assert.NotNull(session);
        Assert.Equal(ImportStatus.Empty, session.ImportStatus);
        Assert.NotNull(session.ImportCompletedAt);
    }

    [Fact]
    public async Task GetCatalogAsync_GroupsSessionsByYearAndMeeting()
    {
        var earlier = NewSession(11730, "Qualifying");
        earlier.DateStart = earlier.DateStart.AddDays(-1);
        earlier.DateEnd = earlier.DateEnd.AddDays(-1);

        var repository = new SessionRepository(_db.Create());
        await repository.UpsertSessionAsync(NewSession(11731));
        await repository.UpsertSessionAsync(earlier);

        var catalog = await repository.GetCatalogAsync(null, null);

        var year = Assert.Single(catalog);
        Assert.Equal(2026, year.Year);
        var meeting = Assert.Single(year.Meetings);
        Assert.Equal(1308, meeting.MeetingKey);
        Assert.Equal("Bahrain Grand Prix", meeting.MeetingName);
        Assert.Equal([11730, 11731], meeting.Sessions.Select(s => s.SessionKey));
    }

    [Fact]
    public async Task GetCatalogAsync_FiltersByType()
    {
        var qualifying = NewSession(11730);
        qualifying.SessionType = "Qualifying";
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));
        await new SessionRepository(_db.Create()).UpsertSessionAsync(qualifying);

        var catalog = await new SessionRepository(_db.Create()).GetCatalogAsync("Qualifying", null);

        var year = Assert.Single(catalog);
        var meeting = Assert.Single(year.Meetings);
        var session = Assert.Single(meeting.Sessions);
        Assert.Equal(11730, session.SessionKey);
    }

    [Fact]
    public async Task GetCatalogAsync_FiltersByYear()
    {
        var older = NewSession(9000);
        older.Year = 2025;
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));
        await new SessionRepository(_db.Create()).UpsertSessionAsync(older);

        var catalog = await new SessionRepository(_db.Create()).GetCatalogAsync(null, 2025);

        var year = Assert.Single(catalog);
        var meeting = Assert.Single(year.Meetings);
        var session = Assert.Single(meeting.Sessions);
        Assert.Equal(9000, session.SessionKey);
    }

    [Fact]
    public async Task GetCatalogAsync_OrdersYearsNewestFirst()
    {
        var older = NewSession(9000);
        older.Year = 2025;
        older.DateStart = new DateTimeOffset(2025, 3, 1, 7, 0, 0, TimeSpan.Zero);
        older.DateEnd = new DateTimeOffset(2025, 3, 1, 9, 0, 0, TimeSpan.Zero);
        await new SessionRepository(_db.Create()).UpsertSessionAsync(older);
        await new SessionRepository(_db.Create()).UpsertSessionAsync(NewSession(11731));

        var catalog = await new SessionRepository(_db.Create()).GetCatalogAsync(null, null);

        Assert.Equal([2026, 2025], catalog.Select(year => year.Year));
        var sessions = catalog.SelectMany(year => year.Meetings).SelectMany(meeting => meeting.Sessions);
        Assert.Equal([11731, 9000], sessions.Select(session => session.SessionKey));
    }
}
