using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryQueryService _telemetryQueryService;

    public TelemetryController(ITelemetryQueryService telemetryQueryService)
    {
        _telemetryQueryService = telemetryQueryService;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetSessions([FromQuery] string? type, [FromQuery] int? year)
    {
        return Ok(await _telemetryQueryService.GetSessionsAsync(type, year));
    }

    [HttpGet("sessions/{sessionKey:int}/laps")]
    public async Task<ActionResult<IReadOnlyList<LapDto>>> GetSessionLaps(int sessionKey)
    {
        var laps = await _telemetryQueryService.GetSessionLapsAsync(sessionKey);
        return laps.Count == 0 ? NotFound() : Ok(laps);
    }
}
