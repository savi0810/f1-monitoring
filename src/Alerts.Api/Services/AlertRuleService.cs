using Alerts.Api.Data.Repositories;

namespace Alerts.Api.Services;

public class AlertRuleService
{
    private readonly IAlertRuleRepository _repository;

    public AlertRuleService(IAlertRuleRepository repository)
    {
        _repository = repository;
    }
}
