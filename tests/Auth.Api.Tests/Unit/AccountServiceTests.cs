using Auth.Api.Data.DTOs;
using Auth.Api.Data.Repositories;
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
}
