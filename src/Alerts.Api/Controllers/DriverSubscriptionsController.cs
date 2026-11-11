using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Alerts.Api.Data.DTOs;
using Alerts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alerts.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts/subscriptions")]
public class DriverSubscriptionsController : ControllerBase
{
    private readonly IDriverSubscriptionService _service;

    public DriverSubscriptionsController(IDriverSubscriptionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DriverSubscriptionDto>>> GetSubscriptions(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || userId == Guid.Empty)
            return Unauthorized();

        return Ok(await _service.GetByUserIdAsync(userId, cancellationToken));
    }

    [HttpPut]
    public async Task<ActionResult<DriverSubscriptionDto>> CreateSubscription(CreateDriverSubscriptionDto request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || userId == Guid.Empty)
            return Unauthorized();

        return Ok(await _service.CreateAsync(userId, request, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteSubscription([FromQuery, Required, StringLength(128)] string driverId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)
            || userId == Guid.Empty)
            return Unauthorized();

        await _service.DeleteAsync(userId, driverId, cancellationToken);

        return NoContent();
    }
}
