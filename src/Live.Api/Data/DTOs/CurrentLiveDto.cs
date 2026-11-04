namespace Live.Api.Data.DTOs;

public record CurrentLiveDto(
    bool IsConnected,
    LiveSnapshotDto? Snapshot);
