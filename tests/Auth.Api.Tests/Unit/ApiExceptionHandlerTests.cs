using Auth.Api.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Auth.Api.Tests.Unit;

public class ApiExceptionHandlerTests
{
    private readonly ApiExceptionHandler _handler = new();

    [Fact]
    public async Task ConflictException_Returns409()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(context, new ConflictException("duplicate"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvalidCredentialsException_Returns401()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var handled = await _handler.TryHandleAsync(context, new InvalidCredentialsException("wrong"), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }
}
