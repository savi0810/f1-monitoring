using System.ComponentModel.DataAnnotations;

namespace Alerts.Api.Data.DTOs;

public record CreateDriverSubscriptionDto([Required, StringLength(128)] string DriverId);
