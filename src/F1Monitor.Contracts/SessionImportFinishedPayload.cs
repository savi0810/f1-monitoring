namespace F1Monitor.Contracts;

public record SessionImportFinishedPayload(Guid ImportId, int LapCount, string? Error);
