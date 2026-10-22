using Auth.Api.Data.Repositories;

namespace Auth.Api.Services;

public class AccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AccountService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }
}
