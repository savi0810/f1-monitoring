using F1Monitor.Contracts;
using Live.Api.Controllers;
using Live.Api.Data.DTOs;
using Live.Api.Data.Repositories;
using Live.Api.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Live.Api.Tests;

public class LiveControllerTests
{
    private readonly ILiveSnapshotRepository _repository;
    private readonly LiveController _controller;

    public LiveControllerTests()
    {
        _repository = Substitute.For<ILiveSnapshotRepository>();
        _controller = new LiveController(new LiveSourceService(_repository));
    }

    [Fact]
    public async Task GetCurrent_NoSnapshot_ReturnsNullSnapshot()
    {
        _repository.GetAsync(null, Arg.Any<CancellationToken>())
            .Returns((LiveRaceSnapshot?)null);

        var result = await _controller.GetCurrent(CancellationToken.None);

        var response = Assert.IsType<CurrentLiveDto>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Null(response.Snapshot);
    }

    [Fact]
    public async Task GetCurrent_Snapshot_ReturnsSnapshot()
    {
        var snapshot = new LiveRaceSnapshot(
            Guid.NewGuid(), "espn", "race",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow, "Race", "Grand Prix", "in",
            false, [], TimeSpan.FromSeconds(120));
        _repository.GetAsync(null, Arg.Any<CancellationToken>()).Returns(snapshot);

        var result = await _controller.GetCurrent(CancellationToken.None);

        var response = Assert.IsType<CurrentLiveDto>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Same(snapshot, response.Snapshot);
    }
}
