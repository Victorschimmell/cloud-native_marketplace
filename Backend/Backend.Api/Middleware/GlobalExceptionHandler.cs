using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Api.Middleware;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (IsDuplicateEmailException(exception))
        {
            var conflictProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "An account with this email already exists."
            };

            httpContext.Response.StatusCode = conflictProblemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(conflictProblemDetails, cancellationToken);

            return true;
        }

        logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            // For development purposes, includes the exception message
            Detail = exception.Message
            // Avoid exposing internal exception details in production environments
            // Detail = "An unexpected error occurred. Please try again later."
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static bool IsDuplicateEmailException(Exception exception)
    {
        if (exception is not DbUpdateException dbUpdateException)
        {
            return false;
        }

        var innerException = dbUpdateException.InnerException;
        var sqlState = innerException?.GetType().GetProperty("SqlState")?.GetValue(innerException)?.ToString();
        var constraintName = innerException?.GetType().GetProperty("ConstraintName")?.GetValue(innerException)?.ToString();

        return sqlState == "23505" &&
            constraintName?.Contains("user_account", StringComparison.OrdinalIgnoreCase) == true &&
            constraintName.Contains("email", StringComparison.OrdinalIgnoreCase);
    }
}
