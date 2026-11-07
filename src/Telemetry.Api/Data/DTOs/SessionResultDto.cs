namespace Telemetry.Api.Data.DTOs;

public record SessionResultDto(
    int DriverNumber,
    string? FullName,
    string? TeamName,
    int? Position,
    int? NumberOfLaps,
    RaceResultDto? Race,
    PracticeResultDto? Practice,
    QualifyingResultDto? Qualifying,
    bool? DidNotFinish,
    bool? DidNotStart,
    bool? Disqualified,
    IReadOnlyList<SessionResultLapDto> Laps);

public record RaceResultDto(
    double? TotalTimeSeconds,
    double? GapToLeaderSeconds,
    string? GapToLeaderText);

public record PracticeResultDto(
    double? BestLapSeconds,
    double? GapToLeaderSeconds);

public record QualifyingResultDto(
    IReadOnlyList<QualifyingPhaseResultDto> Phases);

public record QualifyingPhaseResultDto(
    int Phase,
    double? BestLapSeconds,
    double? GapToLeaderSeconds);

public record SessionResultLapDto(
    int LapNumber,
    double? LapDuration,
    double? Sector1Duration,
    double? Sector2Duration,
    double? Sector3Duration,
    bool IsPitOutLap,
    DateTimeOffset? DateStart);
