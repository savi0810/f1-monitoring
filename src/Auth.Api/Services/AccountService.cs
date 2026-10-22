using Auth.Api.Data.DTOs;
using Auth.Api.Data.Repositories;

namespace Auth.Api.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AccountService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public Task RegisterAsync(RegisterDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<TokenDto> LoginAsync(LoginDto dto)
    {
        throw new NotImplementedException();
    }
}
