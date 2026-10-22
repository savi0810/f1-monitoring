using System.ComponentModel.DataAnnotations;
using Auth.Api.Data.DTOs;

namespace Auth.Api.Tests.Unit;

public class DtoValidationTests
{
    private static bool IsValid(object dto) =>
        Validator.TryValidateObject(dto, new ValidationContext(dto), new List<ValidationResult>(), true);

    [Fact]
    public void RegisterDto_ValidUsername_Passes()
    {
        Assert.True(IsValid(new RegisterDto { Username = "alice", Email = "alice@test.com", Password = "secret1" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void RegisterDto_InvalidUsername_Fails(string username)
    {
        Assert.False(IsValid(new RegisterDto { Username = username, Email = "alice@test.com", Password = "secret1" }));
    }

    [Fact]
    public void RegisterDto_TooLongUsername_Fails()
    {
        Assert.False(IsValid(new RegisterDto { Username = new string('a', 51), Email = "alice@test.com", Password = "secret1" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("alice@")]
    public void RegisterDto_InvalidEmail_Fails(string email)
    {
        Assert.False(IsValid(new RegisterDto { Username = "alice", Email = email, Password = "secret1" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    public void RegisterDto_InvalidPassword_Fails(string password)
    {
        Assert.False(IsValid(new RegisterDto { Username = "alice", Email = "alice@test.com", Password = password }));
    }

    [Fact]
    public void LoginDto_Valid_Passes()
    {
        Assert.True(IsValid(new LoginDto { Email = "alice@test.com", Password = "secret1" }));
    }

    [Theory]
    [InlineData("", "secret1")]
    [InlineData("alice@test.com", "")]
    public void LoginDto_MissingFields_Fails(string email, string password)
    {
        Assert.False(IsValid(new LoginDto { Email = email, Password = password }));
    }
}
