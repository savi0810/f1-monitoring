using System.Net;
using System.Net.Http.Json;
using Auth.Api.Data.DTOs;
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

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwt()
    {
        var name = "u" + Guid.NewGuid().ToString("N")[..10];
        await _client.PostAsJsonAsync("/api/auth/register", NewUser(name));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = name + "@test.com", password = "secret1" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        Assert.Equal(3, token!.AccessToken.Split('.').Length);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var name = "u" + Guid.NewGuid().ToString("N")[..10];
        await _client.PostAsJsonAsync("/api/auth/register", NewUser(name));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = name + "@test.com", password = "wrong-pass" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_UnknownUser_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email = "ghost-" + Guid.NewGuid().ToString("N")[..8] + "@test.com", password = "secret1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
