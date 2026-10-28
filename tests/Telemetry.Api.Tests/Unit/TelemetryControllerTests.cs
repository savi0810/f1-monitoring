using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Telemetry.Api.Controllers;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Tests.Unit;

public class TelemetryControllerTests
{
    private readonly ILapRepository _repository = Substitute.For<ILapRepository>();

    [Fact]
    public async Task GetRaces_ReturnsRacesFromRepository()
    {
        IReadOnlyList<RaceSummary> races = [new RaceSummary(11396, 2, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)];
        _repository.GetRacesAsync().Returns(races);

        var result = await new TelemetryController(_repository).GetRaces();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(races, ok.Value);
    }
}
