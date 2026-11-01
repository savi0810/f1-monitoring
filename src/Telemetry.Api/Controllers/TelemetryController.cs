using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Services;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ILapRepository _lapRepository;
    private readonly ITelemetryQueryService _telemetryQueryService;

    public TelemetryController(ILapRepository lapRepository, ITelemetryQueryService telemetryQueryService)
    {
        _lapRepository = lapRepository;
        _telemetryQueryService = telemetryQueryService;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetSessions([FromQuery] string? type, [FromQuery] int? year)
    {
        return Ok(await _telemetryQueryService.GetSessionsAsync(type, year));
    }

    [HttpGet("sessions/{sessionKey:int}/laps")]
    public async Task<ActionResult<IReadOnlyList<Lap>>> GetSessionLaps(int sessionKey)
    {
        var laps = await _lapRepository.GetLapsBySessionKeyAsync(sessionKey);
        return laps.Count == 0 ? NotFound() : Ok(laps);
    }
}
