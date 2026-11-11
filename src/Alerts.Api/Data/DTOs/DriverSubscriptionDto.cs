namespace Alerts.Api.Data.DTOs;

public record DriverSubscriptionDto(
    Guid Id,
    string DriverId,
    DateTimeOffset CreatedAt);
