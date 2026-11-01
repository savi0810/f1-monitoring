using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryQueryService _telemetryQueryService;
    private readonly ISessionImportService _sessionImportService;

    public TelemetryController(ITelemetryQueryService telemetryQueryService, ISessionImportService sessionImportService)
    {
        _telemetryQueryService = telemetryQueryService;
        _sessionImportService = sessionImportService;
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

    [Authorize]
    [HttpPost("sessions/{sessionKey:int}/import")]
    public async Task<ActionResult<SessionImportDto>> ImportSession(int sessionKey, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _sessionImportService.StartImportAsync(sessionKey, cancellationToken);
            if (result is null)
                return NotFound();

            return result.Status switch
            {
                ImportStatus.Pending or ImportStatus.Processing => Accepted(result),
                ImportStatus.Failed => StatusCode(503, result),
                _ => Ok(result)
            };
        }
        catch (InvalidOperationException)
        {
            return Conflict();
        }
    }
}
