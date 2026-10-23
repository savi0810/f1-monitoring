using Auth.Api.Data.DTOs;
using Auth.Api.Data.Models;
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

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        await _userRepository.AddUserAsync(user);
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email)
            ?? throw new InvalidCredentialsException("Неверный адрес электронной почты или пароль.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidCredentialsException("Неверный адрес электронной почты или пароль.");

        return new TokenDto { AccessToken = _jwtService.GenerateToken(user) };
    }
}
