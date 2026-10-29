using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ILapRepository _lapRepository;
    private readonly ISessionRepository _sessionRepository;

    public TelemetryController(ILapRepository lapRepository, ISessionRepository sessionRepository)
    {
        _lapRepository = lapRepository;
        _sessionRepository = sessionRepository;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionDto>>> GetSessions([FromQuery] string? type, [FromQuery] int? year)
    {
        return Ok(await _sessionRepository.GetSessionsAsync(type, year));
    }

    [HttpGet("sessions/{sessionKey:int}/laps")]
    public async Task<ActionResult<IReadOnlyList<Lap>>> GetSessionLaps(int sessionKey)
    {
        var laps = await _lapRepository.GetLapsBySessionKeyAsync(sessionKey);
        return laps.Count == 0 ? NotFound() : Ok(laps);
    }
}
