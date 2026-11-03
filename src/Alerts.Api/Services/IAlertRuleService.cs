using Alerts.Api.Data.DTOs;

namespace Alerts.Api.Services;

public interface IAlertRuleService
{
    Task<AlertRuleDto> CreateAsync(Guid userId, CreateAlertRuleDto request);
    Task<IReadOnlyList<AlertRuleDto>> GetByUserIdAsync(Guid userId);
    Task<bool> DeleteAsync(Guid ruleId, Guid userId);
}
