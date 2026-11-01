namespace Telemetry.Api.Data.DTOs;

public record SessionImportDto(
    int SessionKey,
    Guid ImportId,
    ImportStatus Status,
    string? Error);
