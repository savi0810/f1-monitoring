using Live.Api.Data.DTOs;
using Live.Api.Data.Repositories;

namespace Live.Api.Services;

public class LiveSourceService
{
    private readonly ILiveSnapshotRepository _repository;

    public LiveSourceService(ILiveSnapshotRepository repository)
    {
        _repository = repository;
    }

    public async Task<CurrentLiveDto> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _repository.GetAsync(null, cancellationToken);

        return new CurrentLiveDto(snapshot);
    }
}
