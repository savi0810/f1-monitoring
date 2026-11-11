using System.Text.Json;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;
using F1Monitor.Contracts;

namespace Alerts.Api.Services;

public class RaceCompletionService : IRaceCompletionService
{
    private readonly ILiveEventRepository _eventRepository;

    public RaceCompletionService(ILiveEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task ProcessAsync(RaceCompleted message, CancellationToken cancellationToken = default)
    {
        if (message == null
            || message.EventId == Guid.Empty
            || string.IsNullOrWhiteSpace(message.SessionId)
            || message.SessionId.Length > 128
            || message.ObservedAt == default
            || message.ObservedAt > DateTimeOffset.UtcNow.AddSeconds(30))
            throw new JsonException("Invalid race completion event.");

        var race = new CompletedRace
        {
            SessionId = message.SessionId,
            EventId = message.EventId,
            ObservedAt = message.ObservedAt
        };

        await _eventRepository.SaveCompletionAsync(race, cancellationToken);
    }
}
