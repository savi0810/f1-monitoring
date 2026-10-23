using Auth.Api.Tests.Infrastructure;

namespace Auth.Api.Tests.Integration;

public class AuthApiTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(AuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object NewUser(string? name = null)
    {
        var username = name ?? "u" + Guid.NewGuid().ToString("N")[..10];
        return new { username, email = username + "@test.com", password = "secret1" };
    }
}
