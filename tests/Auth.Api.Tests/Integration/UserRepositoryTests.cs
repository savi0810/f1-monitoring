using Auth.Api.Data;
using Auth.Api.Tests.Infrastructure;

namespace Auth.Api.Tests.Integration;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();

    public void Dispose() => _db.Dispose();
}
