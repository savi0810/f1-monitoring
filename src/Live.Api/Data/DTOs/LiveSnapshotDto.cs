namespace Live.Api.Data.DTOs;

public record LiveSnapshotDto(
    int SessionKey,
    long Version,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<LiveDriverDto> Drivers);
