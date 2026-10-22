using Auth.Api.Data.DTOs;
using Auth.Api.Data.Models;
using Auth.Api.Data.Repositories;
using Auth.Api.Exceptions;
using Auth.Api.Services;
using NSubstitute;

namespace Auth.Api.Tests.Unit;

public class AccountServiceTests
{
    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly IJwtService _jwt = Substitute.For<IJwtService>();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _service = new AccountService(_repository, _jwt);
    }

    private static RegisterDto Dto() => new() { Username = "alice", Email = "alice@test.com", Password = "secret1" };

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsConflict()
    {
        _repository.GetUserByEmailAsync("alice@test.com").Returns(new User());

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(Dto()));
        await _repository.DidNotReceive().AddUserAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsConflict()
    {
        _repository.GetUserByUsernameAsync("alice").Returns(new User());

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(Dto()));
        await _repository.DidNotReceive().AddUserAsync(Arg.Any<User>());
    }
}
