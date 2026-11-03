using Alerts.Api.Data.Models;

namespace Alerts.Api.Data.Repositories;

public interface IAlertRuleRepository
{
    Task AddAsync(AlertRule rule);
    Task<IReadOnlyList<AlertRule>> GetByUserIdAsync(Guid userId);
    Task<IReadOnlyList<AlertRule>> GetMatchingLapRulesAsync(int driverNumber, double lapTimeSeconds, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid ruleId, Guid userId);
}
