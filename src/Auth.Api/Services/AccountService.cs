using Auth.Api.Data.DTOs;
using Auth.Api.Data.Repositories;
using Auth.Api.Exceptions;

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

    public async Task RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.GetUserByEmailAsync(dto.Email) is not null)
            throw new ConflictException("Пользователь с таким адресом электронной почты уже зарегистрирован.");

        if (await _userRepository.GetUserByUsernameAsync(dto.Username) is not null)
            throw new ConflictException("Пользователь с таким именем уже существует.");

        throw new NotImplementedException();
    }

    public Task<TokenDto> LoginAsync(LoginDto dto)
    {
        throw new NotImplementedException();
    }
}
