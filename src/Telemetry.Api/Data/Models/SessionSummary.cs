namespace Telemetry.Api.Data.Models;

public record SessionSummary(int SessionKey, int LapCount, DateTimeOffset? StartedAt, DateTimeOffset? EndedAt);
