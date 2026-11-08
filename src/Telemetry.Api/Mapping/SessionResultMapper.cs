using System.Text.Json;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;

namespace Telemetry.Api.Mapping;

public static class SessionResultMapper
{
    public static SessionResultDto Map(SessionDriver driver, Session session, IReadOnlyList<Lap> laps)
    {
        RaceResultDto? race = null;
        PracticeResultDto? practice = null;
        QualifyingResultDto? qualifying = null;

        if (session.SessionType.Contains("Qualifying", StringComparison.OrdinalIgnoreCase))
        {
            var durations = ReadArray(driver.DurationJson);
            var gaps = ReadArray(driver.GapToLeaderJson);
            var phaseCount = Math.Max(durations.Count, gaps.Count);

            var phases = Enumerable.Range(0, phaseCount)
                .Select(index => new QualifyingPhaseResultDto(
                    index + 1,
                    index < durations.Count ? durations[index] : null,
                    index < gaps.Count ? gaps[index] : null))
                .ToList();

            qualifying = new QualifyingResultDto(phases);
        }
        else if (session.SessionType.Equals("Race", StringComparison.OrdinalIgnoreCase) ||
                 session.SessionType.Equals("Sprint", StringComparison.OrdinalIgnoreCase))
        {
            race = new RaceResultDto(
                ReadNumber(driver.DurationJson),
                ReadGapSeconds(driver.GapToLeaderJson),
                ReadGapText(driver.GapToLeaderJson));
        }
        else if (session.SessionType.Equals("Practice", StringComparison.OrdinalIgnoreCase))
        {
            practice = new PracticeResultDto(
                ReadNumber(driver.DurationJson),
                ReadNumber(driver.GapToLeaderJson));
        }
        else
        {
            throw new InvalidOperationException($"Unsupported session type: {session.SessionType}");
        }

        return new SessionResultDto(
            driver.DriverNumber,
            driver.FullName,
            driver.TeamName,
            driver.Position,
            driver.NumberOfLaps,
            race,
            practice,
            qualifying,
            driver.DidNotFinish,
            driver.DidNotStart,
            driver.Disqualified,
            laps.Select(LapMapper.ToSessionResultLapDto).ToList());
    }

    private static double? ReadNumber(string? json) =>
        json is null ? null : JsonSerializer.Deserialize<double?>(json);

    private static IReadOnlyList<double?> ReadArray(string? json) =>
        json is null ? [] : JsonSerializer.Deserialize<double?[]>(json) ?? [];

    private static double? ReadGapSeconds(string? json)
    {
        if (json is null)
            return null;

        var value = JsonSerializer.Deserialize<JsonElement>(json);
        return value.ValueKind == JsonValueKind.String ? null : ReadNumber(json);
    }

    private static string? ReadGapText(string? json)
    {
        if (json is null)
            return null;

        var value = JsonSerializer.Deserialize<JsonElement>(json);
        return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    }
}
