using F1Monitor.Contracts;

namespace Alerts.Api.Services;

public interface IPositionAlertService
{
    Task ProcessAsync(DriverPositionsChanged message, CancellationToken cancellationToken = default);
}
