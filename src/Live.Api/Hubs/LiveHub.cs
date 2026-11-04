using Live.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Live.Api.Hubs;

public class LiveHub : Hub
{
    private readonly TimingService _timingService;

    public LiveHub(TimingService timingService)
    {
        _timingService = timingService;
    }
}
