using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.API.Errors;

internal class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred"
        };
        if (exception is GlobalBaseException baseException)
        {
            problemDetails.Title = baseException.Message;
            problemDetails.Status = (int)baseException.StatusCode;
            problemDetails.Extensions["errors"] = baseException.Errors;
        }
        if (exception.InnerException is GlobalBaseException baseInnerException)
        {
            problemDetails.Title = baseInnerException.Message;
            problemDetails.Status = (int)baseInnerException.StatusCode;
            problemDetails.Extensions["errors"] = baseInnerException.Errors;
        }
        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}