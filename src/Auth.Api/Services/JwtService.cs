using System.Security.Claims;
using System.Text;
using Auth.Api.Data.Models;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expirationDays = _configuration.GetValue<int?>("Jwt:ExpirationDays") ?? 7;
        var expires = DateTime.UtcNow.AddDays(expirationDays);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
        };

        throw new NotImplementedException();
    }
}
