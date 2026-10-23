using System.Net;
using System.Net.Http.Json;
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

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", NewUser());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateUser_ReturnsConflict()
    {
        var user = NewUser();
        await _client.PostAsJsonAsync("/api/auth/register", user);

        var response = await _client.PostAsJsonAsync("/api/auth/register", user);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidBody_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { username = "a", email = "bad", password = "1" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
