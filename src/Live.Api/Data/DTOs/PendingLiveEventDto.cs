namespace Live.Api.Data.DTOs;

public record PendingLiveEventDto(
    Guid EventId,
    string RoutingKey,
    string Payload);
