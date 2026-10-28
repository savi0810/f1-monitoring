using System.Text.Json;

namespace F1Monitor.Contracts;

public record RaceEvent(
    Guid EventId,
    string Type,
    int SessionKey,
    int? DriverNumber,
    DateTimeOffset OccurredAt,
    JsonElement Payload,
    string? SessionType = null,
    string? SessionName = null);
