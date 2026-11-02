using Alerts.Api.Data.DTOs;
using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;

namespace Alerts.Api.Services;

public class AlertRuleService
{
    private readonly IAlertRuleRepository _repository;

    public AlertRuleService(IAlertRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<AlertRuleDto> CreateAsync(Guid userId, CreateAlertRuleDto request)
    {
        var rule = new AlertRule
        {
            UserId = userId,
            DriverNumber = request.DriverNumber,
            LapTimeThresholdSeconds = request.LapTimeThresholdSeconds
        };

        await _repository.AddAsync(rule);

        return new AlertRuleDto(rule.Id, rule.DriverNumber, rule.LapTimeThresholdSeconds);
    }
}
