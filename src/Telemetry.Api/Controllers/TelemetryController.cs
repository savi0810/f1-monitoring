using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Publishing;

namespace Telemetry.Api.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ILapRepository _lapRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ImportRequestPublisher _importRequestPublisher;

    public TelemetryController(
        ILapRepository lapRepository,
        ISessionRepository sessionRepository,
        ImportRequestPublisher importRequestPublisher)
    {
        _lapRepository = lapRepository;
        _sessionRepository = sessionRepository;
        _importRequestPublisher = importRequestPublisher;
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
