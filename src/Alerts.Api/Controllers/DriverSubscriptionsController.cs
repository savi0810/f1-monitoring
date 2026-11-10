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
}
