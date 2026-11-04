using Live.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Live.Api.Hubs;

[Authorize]
public class LiveHub : Hub
{
    private readonly TimingService _timingService;

    public LiveHub(TimingService timingService)
    {
        _timingService = timingService;
    }

    public async Task SubscribeSession(int sessionKey)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"session:{sessionKey}");

        var snapshot = await _timingService.GetSnapshotAsync(sessionKey, Context.ConnectionAborted);
        if (snapshot is not null)
            await Clients.Caller.SendAsync("TimingUpdated", snapshot, Context.ConnectionAborted);
    }
}
