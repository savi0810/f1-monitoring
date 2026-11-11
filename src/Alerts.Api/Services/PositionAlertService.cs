using Alerts.Api.Data.Repositories;

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
}
