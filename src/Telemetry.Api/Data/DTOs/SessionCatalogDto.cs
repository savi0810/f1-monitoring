namespace Telemetry.Api.Data.DTOs;

public record SessionCatalogYearDto(
    int Year,
    IReadOnlyList<SessionCatalogMeetingDto> Meetings);

public record SessionCatalogMeetingDto(
    int MeetingKey,
    string? MeetingName,
    IReadOnlyList<SessionDto> Sessions);
