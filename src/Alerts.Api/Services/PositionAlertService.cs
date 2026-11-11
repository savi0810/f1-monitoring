using System.Text.Json;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;
using F1Monitor.Contracts;

namespace Alerts.Api.Services;

public class PositionAlertService : IPositionAlertService
{
    private readonly IDriverSubscriptionRepository _subscriptionRepository;
    private readonly ILiveEventRepository _eventRepository;

    public PositionAlertService(IDriverSubscriptionRepository subscriptionRepository, ILiveEventRepository eventRepository)
    {
        _subscriptionRepository = subscriptionRepository;
        _eventRepository = eventRepository;
    }

    public async Task ProcessAsync(DriverPositionsChanged message, CancellationToken cancellationToken = default)
    {
        if (message == null
            || message.EventId == Guid.Empty
            || string.IsNullOrWhiteSpace(message.SessionId)
            || message.SessionId.Length > 128
            || message.ObservedAt == default
            || message.Changes == null
            || message.Changes.Count == 0)
            throw new JsonException("Invalid position change event.");

        var changes = new Dictionary<string, DriverPositionChange>(StringComparer.Ordinal);

        foreach (var change in message.Changes)
        {
            if (change == null
                || string.IsNullOrWhiteSpace(change.DriverId)
                || change.DriverId.Length > 128
                || string.IsNullOrWhiteSpace(change.DriverName)
                || change.PreviousPosition <= 0
                || change.CurrentPosition <= 0
                || change.PreviousPosition == change.CurrentPosition
                || !changes.TryAdd(change.DriverId, change))
                throw new JsonException("Invalid or duplicate driver position change.");
        }

        var subscriptions = await _subscriptionRepository.GetSubscribersAsync(
            changes.Keys.ToArray(), message.ObservedAt, cancellationToken);

        var messages = new List<OutboxMessage>();

        foreach (var subscription in subscriptions)
        {
            var change = changes[subscription.DriverId];
            var outboxMessage = new OutboxMessage();
            var alert = new PositionAlertTriggered(
                outboxMessage.Id,
                subscription.UserId,
                subscription.Id,
                message.EventId,
                message.SessionId,
                change.DriverId,
                change.DriverName,
                change.PreviousPosition,
                change.CurrentPosition,
                message.ObservedAt);

            outboxMessage.Payload = EventSerializer.Serialize(alert);
            messages.Add(outboxMessage);
        }

        await _eventRepository.SaveAsync(message.EventId, messages, cancellationToken);
    }
}
