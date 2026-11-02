using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Telemetry.Api.Controllers;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Services;

namespace Telemetry.Api.Tests.Unit;

public class TelemetryControllerTests
{
    private readonly ITelemetryQueryService _queryService = Substitute.For<ITelemetryQueryService>();
    private readonly ISessionImportService _importService = Substitute.For<ISessionImportService>();

    [Fact]
    public async Task GetSessions_ReturnsSessionsFromSessionRepository()
    {
        IReadOnlyList<SessionDto> sessions =
        [
            new SessionDto(11731, 1308, 2026, "Race", "Race", "Kuala Lumpur", "Bahrain", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, false, 1145)
        ];
        _queryService.GetSessionsAsync("Race", 2026).Returns(sessions);

        var result = await new TelemetryController(_queryService, _importService).GetSessions("Race", 2026);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(sessions, ok.Value);
    }

    [Fact]
    public async Task GetSessionLaps_KnownSession_ReturnsLaps()
    {
        IReadOnlyList<LapDto> laps = [new(Guid.NewGuid(), 11396, null, 1, null, null, null, null, false, null)];
        _queryService.GetSessionLapsAsync(11396).Returns(laps);

        var result = await new TelemetryController(_queryService, _importService).GetSessionLaps(11396);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(laps, ok.Value);
    }

    [Fact]
    public async Task GetSessionLaps_UnknownSession_ReturnsNotFound()
    {
        _queryService.GetSessionLapsAsync(1).Returns(Array.Empty<LapDto>());

        var result = await new TelemetryController(_queryService, _importService).GetSessionLaps(1);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ImportSession_PendingImport_ReturnsAccepted()
    {
        var import = new SessionImportDto(11396, Guid.NewGuid(), ImportStatus.Pending, null);
        _importService.StartImportAsync(11396, Arg.Any<CancellationToken>()).Returns(import);

        var result = await new TelemetryController(_queryService, _importService)
            .ImportSession(11396, CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result.Result);
        Assert.Same(import, accepted.Value);
    }

    [Fact]
    public async Task GetImportStatus_ExistingImport_ReturnsImport()
    {
        var import = new SessionImportDto(11396, Guid.NewGuid(), ImportStatus.Pending, null);
        _importService.GetImportStatusAsync(11396, Arg.Any<CancellationToken>())
            .Returns((true, import));

        var result = await new TelemetryController(_queryService, _importService)
            .GetImportStatus(11396, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(import, ok.Value);
    }

    [Fact]
    public async Task GetImportStatus_NoImport_ReturnsOkWithNull()
    {
        _importService.GetImportStatusAsync(11396, Arg.Any<CancellationToken>())
            .Returns((true, (SessionImportDto?)null));

        var result = await new TelemetryController(_queryService, _importService)
            .GetImportStatus(11396, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Null(ok.Value);
    }

    [Fact]
    public async Task GetImportStatus_UnknownSession_ReturnsNotFound()
    {
        _importService.GetImportStatusAsync(1, Arg.Any<CancellationToken>())
            .Returns((false, (SessionImportDto?)null));

        var result = await new TelemetryController(_queryService, _importService)
            .GetImportStatus(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
