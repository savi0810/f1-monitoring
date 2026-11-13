using Auth.Api.Data.DTOs;
using Auth.Api.Data.Models;
using Auth.Api.Data.Repositories;
using Auth.Api.Exceptions;

namespace Auth.Api.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IUserRepository userRepository, IJwtService jwtService, ILogger<AccountService> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _logger = logger;
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
        _logger.LogInformation("User {UserId} registered", user.Id);
    }

    public async Task<TokenDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetUserByEmailAsync(dto.Email);
        if (user is null)
        {
            _logger.LogWarning("Login rejected: invalid credentials");
            throw new InvalidCredentialsException("Неверный адрес электронной почты или пароль.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login rejected for user {UserId}: invalid credentials", user.Id);
            throw new InvalidCredentialsException("Неверный адрес электронной почты или пароль.");
        }

        var token = _jwtService.GenerateToken(user);
        _logger.LogInformation("User {UserId} logged in", user.Id);

        return new TokenDto { AccessToken = token };
    }
}
