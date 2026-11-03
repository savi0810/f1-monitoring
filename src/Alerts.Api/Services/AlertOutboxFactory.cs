using Alerts.Api.Data.Models;
using F1Monitor.Contracts;

namespace Alerts.Api.Services;

public static class AlertOutboxFactory
{
    public static OutboxMessage Create(AlertRule rule, Guid eventId, double lapTimeSeconds)
    {
        var message = new OutboxMessage();
        var alert = new AlertTriggered(
            message.Id,
            rule.UserId,
            rule.Id,
            eventId,
            rule.DriverNumber,
            lapTimeSeconds);

        message.Payload = EventSerializer.Serialize(alert);
        return message;
    }
}
