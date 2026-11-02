using System.Security.Claims;
using Alerts.Api.Data.DTOs;
using Alerts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alerts.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts/rules")]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleService _service;

    public AlertRulesController(IAlertRuleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlertRuleDto>>> GetRules()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        return Ok(await _service.GetByUserIdAsync(userId));
    }
}
