namespace F1Monitor.Contracts;

public record SessionDriverPayload(
    string? FullName,
    string? TeamName,
    int? Position,
    int? NumberOfLaps,
    string? DurationJson,
    string? GapToLeaderJson,
    bool? DidNotFinish,
    bool? DidNotStart,
    bool? Disqualified);
