using Alerts.Api.Data.Repositories;

namespace Alerts.Api.Services;

public class DriverSubscriptionService
{
    private readonly IDriverSubscriptionRepository _repository;

    public DriverSubscriptionService(IDriverSubscriptionRepository repository)
    {
        _repository = repository;
    }
}
