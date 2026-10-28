using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ILapRepository _lapRepository;

    public TelemetryController(ILapRepository lapRepository)
    {
        _lapRepository = lapRepository;
    }
}
