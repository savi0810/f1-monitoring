using F1Monitor.Contracts;
using Telemetry.Api.Data.DTOs;
using Telemetry.Api.Data.Models;
using Telemetry.Api.Data.Repositories;
using Telemetry.Api.Publishing;

namespace Telemetry.Api.Services;

public class SessionImportService : ISessionImportService
{
    private readonly ISessionRepository _sessionRepository;
    private readonly ImportRequestPublisher _importRequestPublisher;
    private readonly ILogger<SessionImportService> _logger;

    public SessionImportService(ISessionRepository sessionRepository, ImportRequestPublisher importRequestPublisher, ILogger<SessionImportService> logger)
    {
        _sessionRepository = sessionRepository;
        _importRequestPublisher = importRequestPublisher;
        _logger = logger;
    }

    public async Task<SessionImportDto?> StartImportAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByKeyAsync(sessionKey, cancellationToken);

        if (session is null)
            return null;

        if (session.IsCancelled)
            throw new InvalidOperationException("Cancelled sessions cannot be imported.");

        return await StartImportForSessionAsync(session, cancellationToken);
    }

    public async Task<(bool SessionExists, SessionImportDto? Import)> GetImportStatusAsync(int sessionKey, CancellationToken cancellationToken = default)
    {
        var session = await _sessionRepository.GetByKeyAsync(sessionKey, cancellationToken);

        if (session is null)
            return (false, null);

        if (session.ImportId is not Guid importId || session.ImportStatus is not ImportStatus status)
            return (true, null);

        return (true, new(session.SessionKey, importId, status, session.ImportError));
    }

    private async Task<SessionImportDto> StartImportForSessionAsync(Session session, CancellationToken cancellationToken)
    {
        if (session.ImportId is Guid existingId && session.ImportStatus is ImportStatus existingStatus
            && existingStatus is (ImportStatus.Pending or ImportStatus.Completed or ImportStatus.Empty))
            return new(session.SessionKey, existingId, existingStatus, session.ImportError);

        var importId = Guid.NewGuid();

        if (!await _sessionRepository.TryBeginImportAsync(session.SessionKey, importId, cancellationToken))
            return await GetCurrentImportOrThrowAsync(session.SessionKey, cancellationToken);

        try
        {
            await _importRequestPublisher.PublishAsync(new ImportRequested(session.SessionKey, importId), cancellationToken);
        }
        catch (Exception exception)
        {
            if (exception is not OperationCanceledException)
                _logger.LogError(exception, "Failed to queue import {ImportId} for session {SessionKey}", importId, session.SessionKey);

            await _sessionRepository.MarkImportFailedAsync(session.SessionKey, importId, exception.Message, CancellationToken.None);
            if (exception is OperationCanceledException)
                throw;

            return new(session.SessionKey, importId, ImportStatus.Failed, "The import request could not be queued.");
        }

        _logger.LogInformation("Import {ImportId} queued for session {SessionKey}", importId, session.SessionKey);

        return new(session.SessionKey, importId, ImportStatus.Pending, null);
    }

    private async Task<SessionImportDto> GetCurrentImportOrThrowAsync(int sessionKey, CancellationToken cancellationToken)
    {
        var current = await _sessionRepository.GetByKeyAsync(sessionKey, cancellationToken);

        if (current is null || current.ImportId is not Guid importId || current.ImportStatus is not ImportStatus status)
            throw new InvalidOperationException("The session import state changed before it could be started.");

        return new(sessionKey, importId, status, current.ImportError);
    }
}
