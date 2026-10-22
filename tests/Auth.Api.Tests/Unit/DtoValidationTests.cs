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
        Assert.True(IsValid(new RegisterDto { Username = "alice", Email = "alice@test.com" }));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    public void RegisterDto_InvalidUsername_Fails(string username)
    {
        Assert.False(IsValid(new RegisterDto { Username = username, Email = "alice@test.com" }));
    }

    [Fact]
    public void RegisterDto_TooLongUsername_Fails()
    {
        Assert.False(IsValid(new RegisterDto { Username = new string('a', 51), Email = "alice@test.com" }));
    }
}
