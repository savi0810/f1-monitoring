using F1Monitor.Contracts;

namespace Live.Api.Data.DTOs;

public record CurrentLiveDto(
    LiveRaceSnapshot? Snapshot);
