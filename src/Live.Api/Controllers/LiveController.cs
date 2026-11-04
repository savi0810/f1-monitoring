using Live.Api.Data.DTOs;
using Live.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Live.Api.Controllers;

[ApiController]
[Route("api/live")]
[Authorize]
public class LiveController : ControllerBase
{
    private readonly LiveSourceService _sourceService;

    public LiveController(LiveSourceService sourceService)
    {
        _sourceService = sourceService;
    }

    [HttpGet("current")]
    public async Task<ActionResult<CurrentLiveDto>> GetCurrent(CancellationToken cancellationToken)
    {
        return Ok(await _sourceService.GetCurrentAsync(cancellationToken));
    }
}
