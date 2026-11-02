using System.ComponentModel.DataAnnotations;

namespace Alerts.Api.Data.DTOs;

public class CreateAlertRuleDto
{
    [Range(1, int.MaxValue)]
    public int DriverNumber { get; set; }

    [Range(double.Epsilon, double.MaxValue)]
    public double LapTimeThresholdSeconds { get; set; }
}
