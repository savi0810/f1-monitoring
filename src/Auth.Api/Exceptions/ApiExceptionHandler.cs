using Microsoft.AspNetCore.Diagnostics;

namespace Auth.Api.Exceptions;

public class ApiExceptionHandler : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(false);
    }
}
