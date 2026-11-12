using F1Monitor.Contracts;
using Live.Api.Controllers;
using Live.Api.Data.DTOs;
using Live.Api.Data.Repositories;
using Live.Api.Hubs;
using Live.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        var timing = new TimingService(
            _repository,
            Substitute.For<IHubContext<LiveHub>>(),
            new PositionChangeDetector(),
            new RaceCompletionDetector());
        _controller = new LiveController(new LiveSourceService(timing));
    }

    [Fact]
    public async Task GetCurrent_NoSnapshot_ReturnsDisconnected()
    {
        _repository.GetAsync(null, Arg.Any<CancellationToken>())
            .Returns((LiveRaceSnapshot?)null);

        var result = await _controller.GetCurrent(CancellationToken.None);

        var response = Assert.IsType<CurrentLiveDto>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.False(response.IsConnected);
        Assert.Null(response.Snapshot);
    }

    [Theory]
    [InlineData(5, 0, true)]
    [InlineData(120, 0, false)]
    [InlineData(60, 120, true)]
    public async Task GetCurrent_Snapshot_ReturnsConnectionStatus(int ageSeconds, int intervalSeconds, bool expectedConnected)
    {
        var snapshot = new LiveRaceSnapshot(
            Guid.NewGuid(), "espn", "race",
            DateTimeOffset.UtcNow.AddSeconds(-ageSeconds),
            DateTimeOffset.UtcNow, "Race", "Grand Prix", "in",
            false, "", [], null, TimeSpan.FromSeconds(intervalSeconds));
        _repository.GetAsync(null, Arg.Any<CancellationToken>()).Returns(snapshot);

        var result = await _controller.GetCurrent(CancellationToken.None);

        var response = Assert.IsType<CurrentLiveDto>(
            Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(expectedConnected, response.IsConnected);
        Assert.Same(snapshot, response.Snapshot);
    }
}
