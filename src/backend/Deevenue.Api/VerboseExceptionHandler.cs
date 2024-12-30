using System.Net;
using Microsoft.AspNetCore.Diagnostics;

namespace Deevenue.Api;

internal class VerboseExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
                {
                    Title = "Caught exception",
                    Detail = exception.Message,
                    Type = exception.GetType().Name,
                },
            Exception = exception
        });
    }
}
