using Auth.Api.Data.Models;
using Auth.Api.Services;
using Microsoft.Extensions.Configuration;

namespace Auth.Api.Tests.Unit;

public class JwtServiceTests
{
    private static JwtService Create(Dictionary<string, string?> values) =>
        new(new ConfigurationBuilder().AddInMemoryCollection(values).Build());

    private static User User() => new() { Username = "alice", Email = "alice@test.com" };

    [Fact]
    public void GenerateToken_WithoutKey_Throws()
    {
        var service = Create(new());

        Assert.Throws<InvalidOperationException>(() => service.GenerateToken(User()));
    }
}
