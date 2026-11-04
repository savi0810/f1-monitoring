using Live.Api.Data.Repositories;

namespace Live.Api.Services;

public class TimingService
{
    private readonly ILiveSnapshotRepository _repository;

    public TimingService(ILiveSnapshotRepository repository)
    {
        _repository = repository;
    }
}
