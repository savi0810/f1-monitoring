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
}
