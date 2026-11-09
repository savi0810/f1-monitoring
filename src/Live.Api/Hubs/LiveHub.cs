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

    public async Task SubscribeSession(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || sessionId.Length > 200)
            throw new HubException("Invalid session identifier.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"session:{sessionId}");

        var snapshot = await _timingService.GetSnapshotAsync(sessionId, Context.ConnectionAborted);
        if (snapshot != null)
            await Clients.Caller.SendAsync("TimingUpdated", snapshot, Context.ConnectionAborted);
    }
}
