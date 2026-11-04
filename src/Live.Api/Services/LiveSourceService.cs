using F1Monitor.Contracts;
using Live.Api.Data.Models;
using Live.Api.Data.Repositories;

namespace Live.Api.Services;

public class LiveSourceService
{
    private readonly ILiveSourceStateRepository _repository;

    public LiveSourceService(ILiveSourceStateRepository repository)
    {
        _repository = repository;
    }

    public Task ProcessAsync(LiveSourceStatus status, CancellationToken cancellationToken = default)
    {
        var state = new LiveSourceState
        {
            IsConnected = status.IsConnected,
            SessionKey = status.SessionKey,
            ReportedAt = status.ReportedAt
        };

        return _repository.SaveAsync(state, cancellationToken);
    }
}
