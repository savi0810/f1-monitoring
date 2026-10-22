using Auth.Api.Data;
using Auth.Api.Data.Models;
using Auth.Api.Data.Repositories;
using Auth.Api.Tests.Infrastructure;

namespace Auth.Api.Tests.Integration;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteDb<AppDbContext> _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddUserAsync_PersistsUser()
    {
        var user = new User { Username = "alice", Email = "alice@test.com", PasswordHash = "hash" };

        await new UserRepository(_db.Create()).AddUserAsync(user);

        Assert.NotNull(await new UserRepository(_db.Create()).GetUserByUsernameAsync("alice"));
    }

    [Fact]
    public async Task GetUserByUsernameAsync_Unknown_ReturnsNull()
    {
        Assert.Null(await new UserRepository(_db.Create()).GetUserByUsernameAsync("ghost"));
    }
}
