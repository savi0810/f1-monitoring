using Alerts.Api.Data.Models;
using Alerts.Api.Data.Repositories;
using F1Monitor.Contracts;

namespace Alerts.Api.Services;

public class LapRuleMatcher
{
    private readonly IAlertRuleRepository _repository;

    public LapRuleMatcher(IAlertRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AlertRule>> MatchAsync(RaceEvent raceEvent, CancellationToken cancellationToken = default)
    {
        if (raceEvent.Type != EventTypes.LapCompleted || raceEvent.DriverNumber is null)
            return [];

        var payload = EventSerializer.ReadPayload<LapCompletedPayload>(raceEvent);
        if (payload.IsPitOutLap || payload.LapDuration is null)
            return [];

        return await _repository.GetMatchingLapRulesAsync(raceEvent.DriverNumber.Value, payload.LapDuration.Value, cancellationToken);
    }
}
