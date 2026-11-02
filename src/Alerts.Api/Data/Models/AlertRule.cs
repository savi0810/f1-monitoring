namespace Alerts.Api.Data.Models;

public class AlertRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int DriverNumber { get; set; }
    public double LapTimeThresholdSeconds { get; set; }
}
