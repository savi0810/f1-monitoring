using Microsoft.AspNetCore.Mvc;
using Telemetry.Api.Data.Models;
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

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<SessionSummary>>> GetSessions()
    {
        return Ok(await _lapRepository.GetSessionsAsync());
    }

    [HttpGet("races/{sessionKey:int}")]
    public async Task<ActionResult<IReadOnlyList<Lap>>> GetRace(int sessionKey)
    {
        var laps = await _lapRepository.GetLapsBySessionKeyAsync(sessionKey);
        return laps.Count == 0 ? NotFound() : Ok(laps);
    }
}
