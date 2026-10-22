using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Api.Data.Models;
using Auth.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Tests.Unit;

public class JwtServiceTests
{
    private const string Key = "0123456789abcdef0123456789abcdef";

    private static JwtService Create(Dictionary<string, string?> values) =>
        new(new ConfigurationBuilder().AddInMemoryCollection(values).Build());

    private static Dictionary<string, string?> BaseConfig() => new()
    {
        ["Jwt:Key"] = Key,
        ["Jwt:Issuer"] = "issuer",
        ["Jwt:Audience"] = "audience"
    };

    private static User User() => new() { Username = "alice", Email = "alice@test.com" };

    [Fact]
    public void GenerateToken_WithoutKey_Throws()
    {
        var service = Create(new());

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(User()));
    }

    [Fact]
    public void GenerateToken_ProducesValidTokenWithClaims()
    {
        var user = User();
        var token = Create(BaseConfig()).GenerateToken(user);

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidIssuer = "issuer",
            ValidAudience = "audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key))
        }, out _);

        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal("alice", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("alice@test.com", principal.FindFirst(ClaimTypes.Email)?.Value);
    }

    [Fact]
    public void GenerateToken_WithoutExpiration_DefaultsToSevenDays()
    {
        var token = Create(BaseConfig()).GenerateToken(User());

        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        Assert.InRange(validTo, DateTime.UtcNow.AddDays(7).AddMinutes(-1), DateTime.UtcNow.AddDays(7).AddMinutes(1));
    }

    [Fact]
    public void GenerateToken_UsesConfiguredExpirationDays()
    {
        var config = BaseConfig();
        config["Jwt:ExpirationDays"] = "30";

        var token = Create(config).GenerateToken(User());

        var validTo = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo;
        Assert.InRange(validTo, DateTime.UtcNow.AddDays(30).AddMinutes(-1), DateTime.UtcNow.AddDays(30).AddMinutes(1));
    }
}
