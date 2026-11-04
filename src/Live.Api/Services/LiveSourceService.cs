using Live.Api.Data.Repositories;

namespace Live.Api.Services;

public class LiveSourceService
{
    private readonly ILiveSourceStateRepository _repository;

    public LiveSourceService(ILiveSourceStateRepository repository)
    {
        _repository = repository;
    }
}
