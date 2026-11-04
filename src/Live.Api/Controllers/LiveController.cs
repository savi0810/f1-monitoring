using Live.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Live.Api.Controllers;

[ApiController]
[Route("api/live")]
public class LiveController : ControllerBase
{
    private readonly LiveSourceService _sourceService;

    public LiveController(LiveSourceService sourceService)
    {
        _sourceService = sourceService;
    }
}
