using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Telemetry.Api.Controllers;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Tests.Unit;

public class TelemetryControllerTests
{
    private readonly ILapRepository _repository = Substitute.For<ILapRepository>();
    private readonly ISessionRepository _sessionRepository = Substitute.For<ISessionRepository>();

    [Fact]
    public async Task GetSessions_ReturnsSessionsFromSessionRepository()
    {
        IReadOnlyList<SessionDto> sessions =
        [
            new SessionDto(11731, 1308, 2026, "Race", "Race", "Kuala Lumpur", "Bahrain", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, false, 1145)
        ];
        _sessionRepository.GetSessionsAsync("Race", 2026).Returns(sessions);

        var result = await new TelemetryController(_repository, _sessionRepository).GetSessions("Race", 2026);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(sessions, ok.Value);
    }

    [Fact]
    public async Task GetSessionLaps_KnownSession_ReturnsLaps()
    {
        IReadOnlyList<Lap> laps = [new Lap { EventId = Guid.NewGuid(), SessionKey = 11396, LapNumber = 1 }];
        _repository.GetLapsBySessionKeyAsync(11396).Returns(laps);

        var result = await new TelemetryController(_repository, _sessionRepository).GetSessionLaps(11396);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(laps, ok.Value);
    }

    [Fact]
    public async Task GetSessionLaps_UnknownSession_ReturnsNotFound()
    {
        _repository.GetLapsBySessionKeyAsync(1).Returns(Array.Empty<Lap>());

        var result = await new TelemetryController(_repository, _sessionRepository).GetSessionLaps(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
