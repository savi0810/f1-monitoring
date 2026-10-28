namespace Telemetry.Api.Data.Models;

public record RaceSummary(int SessionKey, int LapCount, DateTimeOffset StartedAt, DateTimeOffset EndedAt);
