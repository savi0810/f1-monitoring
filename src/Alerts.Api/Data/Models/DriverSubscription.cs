namespace Alerts.Api.Data.Models;

public class DriverSubscription
{
    public DriverSubscription()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DriverId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
