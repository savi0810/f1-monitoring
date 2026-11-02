namespace Alerts.Api.Data.DTOs;

public record AlertRuleDto(
    Guid Id,
    int DriverNumber,
    double LapTimeThresholdSeconds);
