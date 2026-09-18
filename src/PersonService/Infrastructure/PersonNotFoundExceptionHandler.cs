using Microsoft.AspNetCore.Diagnostics;
using PersonService.Dto;
using PersonService.Exceptions;

namespace PersonService.Infrastructure;

public class PersonNotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not PersonNotFoundException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        await httpContext.Response.WriteAsJsonAsync(new ErrorResponse(exception.Message), cancellationToken);
        return true;
    }
}
