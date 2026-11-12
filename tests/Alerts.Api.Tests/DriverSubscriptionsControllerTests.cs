using System.Security.Claims;
using Alerts.Api.Controllers;
using Alerts.Api.Data;
using Alerts.Api.Data.DTOs;
using Alerts.Api.Data.Repositories;
using Alerts.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Alerts.Api.Tests;

public class DriverSubscriptionsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _context;
    private readonly DriverSubscriptionsController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public DriverSubscriptionsControllerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection).Options;
        _context = new AppDbContext(options);
        _context.Database.EnsureCreated();

        var repository = new DriverSubscriptionRepository(_context);
        var service = new DriverSubscriptionService(repository);
        _controller = new DriverSubscriptionsController(service);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, _userId.ToString())],
                    "test"))
            }
        };
    }

    [Fact]
    public async Task Subscriptions_CreateReadAndDelete_PreservesUserAndAvoidsDuplicates()
    {
        var request = new CreateDriverSubscriptionDto("1");

        var created = await _controller.CreateSubscription(request, CancellationToken.None);
        var subscription = Assert.IsType<DriverSubscriptionDto>(
            Assert.IsType<OkObjectResult>(created.Result).Value);
        Assert.Equal("1", subscription.DriverId);

        var repeated = await _controller.CreateSubscription(request, CancellationToken.None);
        var duplicate = Assert.IsType<DriverSubscriptionDto>(
            Assert.IsType<OkObjectResult>(repeated.Result).Value);
        Assert.Equal(subscription.Id, duplicate.Id);
        Assert.Equal(_userId, (await _context.DriverSubscriptions.SingleAsync()).UserId);

        var listed = await _controller.GetSubscriptions(CancellationToken.None);
        var subscriptions = Assert.IsAssignableFrom<IReadOnlyList<DriverSubscriptionDto>>(
            Assert.IsType<OkObjectResult>(listed.Result).Value);
        Assert.Equal(subscription, Assert.Single(subscriptions));

        var deleted = await _controller.DeleteSubscription("1", CancellationToken.None);
        Assert.IsType<NoContentResult>(deleted);

        var remaining = await _controller.GetSubscriptions(CancellationToken.None);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<DriverSubscriptionDto>>(
            Assert.IsType<OkObjectResult>(remaining.Result).Value));
        Assert.Empty(await _context.DriverSubscriptions.ToListAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Subscriptions_InvalidUserClaim_ReturnsUnauthorized(string? userId)
    {
        Claim[] claims = userId is null
            ? []
            : [new Claim(ClaimTypes.NameIdentifier, userId)];
        _controller.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var listed = await _controller.GetSubscriptions(CancellationToken.None);
        var created = await _controller.CreateSubscription(
            new CreateDriverSubscriptionDto("1"), CancellationToken.None);
        var deleted = await _controller.DeleteSubscription("1", CancellationToken.None);

        Assert.IsType<UnauthorizedResult>(listed.Result);
        Assert.IsType<UnauthorizedResult>(created.Result);
        Assert.IsType<UnauthorizedResult>(deleted);
        Assert.Empty(await _context.DriverSubscriptions.ToListAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
