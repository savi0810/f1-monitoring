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

    private static LoginDto LoginDto() => new() { Email = "alice@test.com", Password = "secret1" };

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

    [Fact]
    public async Task RegisterAsync_SavesUserWithHashedPassword()
    {
        User? saved = null;
        await _repository.AddUserAsync(Arg.Do<User>(u => saved = u));

        await _service.RegisterAsync(Dto());

        Assert.NotNull(saved);
        Assert.Equal("alice", saved!.Username);
        Assert.NotEqual("secret1", saved.PasswordHash);
        Assert.True(BCrypt.Net.BCrypt.Verify("secret1", saved.PasswordHash));
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ThrowsInvalidCredentials()
    {
        _repository.GetUserByEmailAsync("alice@test.com").Returns((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _service.LoginAsync(LoginDto()));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentials()
    {
        var user = new User { Email = "alice@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1") };
        _repository.GetUserByEmailAsync("alice@test.com").Returns(user);

        var dto = new LoginDto { Email = "alice@test.com", Password = "wrong-password" };

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _service.LoginAsync(dto));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        var user = new User { Email = "alice@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("secret1") };
        _repository.GetUserByEmailAsync("alice@test.com").Returns(user);
        _jwt.GenerateToken(user).Returns("token");

        var result = await _service.LoginAsync(LoginDto());

        Assert.Equal("token", result.AccessToken);
    }
}
