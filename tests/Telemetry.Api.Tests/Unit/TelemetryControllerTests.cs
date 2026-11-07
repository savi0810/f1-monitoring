using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
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
    public async Task GetSessionResults_KnownSession_ReturnsResults()
    {
        IReadOnlyList<SessionResultDto> results =
        [
            new(1, "Max Verstappen", "Red Bull Racing", 1, 57,
                new RaceResultDto(5400.123, 0, null), null, null,
                false, false, false, [])
        ];
        _queryService.SessionExistsAsync(11396).Returns(true);
        _queryService.GetSessionResultsAsync(11396).Returns(results);

        var result = await new TelemetryController(_queryService, _importService)
            .GetSessionResults(11396);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(results, ok.Value);
    }

    [Fact]
    public async Task GetSessionResults_UnknownSession_ReturnsNotFound()
    {
        _queryService.SessionExistsAsync(1).Returns(false);

        var result = await new TelemetryController(_queryService, _importService)
            .GetSessionResults(1);

        Assert.IsType<NotFoundResult>(result.Result);
        await _queryService.DidNotReceive().GetSessionResultsAsync(1);
    }

    [Fact]
    public async Task GetSessions_ReturnsGroupedCatalog()
    {
        IReadOnlyList<SessionCatalogYearDto> catalog =
        [
            new SessionCatalogYearDto(2026,
            [
                new SessionCatalogMeetingDto(1308, "Bahrain Grand Prix",
                [
                    new SessionDto(11731, 1308, 2026, "Race", "Race", "Kuala Lumpur", "Bahrain", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, false, 1145)
                ])
            ])
        ];
        _queryService.GetCatalogAsync("Race", 2026).Returns(catalog);

        var result = await new TelemetryController(_queryService, _importService).GetSessions("Race", 2026);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(catalog, ok.Value);
    }

    [Fact]
    public async Task GetSessionLaps_KnownSession_ReturnsLaps()
    {
        IReadOnlyList<LapDto> laps = [new(Guid.NewGuid(), 11396, null, 1, null, null, null, null, false, null)];
        _queryService.SessionExistsAsync(11396).Returns(true);
        _queryService.GetSessionLapsAsync(11396).Returns(laps);

        var result = await new TelemetryController(_queryService, _importService).GetSessionLaps(11396);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(laps, ok.Value);
    }

    [Fact]
    public async Task GetSessionLaps_KnownSessionWithoutLaps_ReturnsEmptyList()
    {
        _queryService.SessionExistsAsync(11396).Returns(true);
        _queryService.GetSessionLapsAsync(11396).Returns(Array.Empty<LapDto>());

        var result = await new TelemetryController(_queryService, _importService).GetSessionLaps(11396);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<LapDto>>(ok.Value));
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

        var url = Substitute.For<IUrlHelper>();
        url.Action(Arg.Any<UrlActionContext>()).Returns("/api/telemetry/sessions/11396/import");
        var controller = new TelemetryController(_queryService, _importService) { Url = url };

        var result = await controller.ImportSession(11396, CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result.Result);
        Assert.Equal("/api/telemetry/sessions/11396/import", accepted.Location);
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
