using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Backend.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment hostEnvironment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        var method = httpContext.Request.Method;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var userId = Guid.TryParse(userIdClaim, out var parsed) ? parsed : (Guid?)null;

        int statusCode;
        string title;
        string safeDetail;

        switch (exception)
        {
            case NotFoundException notFound:
                statusCode = StatusCodes.Status404NotFound;
                title = "Not Found";
                safeDetail = notFound.Message;
                logger.LogWarning(
                    notFound,
                    "{Message}. TraceId={TraceId} Method={Method} Path={Path} UserId={UserId}",
                    notFound.Message, traceId, method, path, userId);
                break;

            case ForbiddenException forbidden:
                statusCode = StatusCodes.Status403Forbidden;
                title = "Forbidden";
                safeDetail = forbidden.Message;
                logger.LogWarning(
                    forbidden,
                    "{Message}. TraceId={TraceId} Method={Method} Path={Path} UserId={UserId}",
                    forbidden.Message, traceId, method, path, userId);
                break;

            case ConflictException conflict:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflict";
                safeDetail = conflict.Message;
                logger.LogWarning(
                    conflict,
                    "{Message}. TraceId={TraceId} Method={Method} Path={Path} UserId={UserId}",
                    conflict.Message, traceId, method, path, userId);
                break;

            case MarketplaceException marketplace:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Bad Request";
                safeDetail = marketplace.Message;
                logger.LogWarning(
                    marketplace,
                    "{Message}. TraceId={TraceId} Method={Method} Path={Path} UserId={UserId}",
                    marketplace.Message, traceId, method, path, userId);
                break;

            default:
                statusCode = StatusCodes.Status500InternalServerError;
                title = "Internal Server Error";
                safeDetail = hostEnvironment.IsDevelopment()
                    ? exception.Message
                    : "An unexpected error occurred. Please try again later.";
                logger.LogError(
                    exception,
                    "Unhandled exception. TraceId={TraceId} Method={Method} Path={Path} StatusCode={StatusCode} UserId={UserId}",
                    traceId, method, path, statusCode, userId);
                break;
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = safeDetail,
            Instance = path,
            Extensions = { ["traceId"] = traceId }
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, options: null, contentType: "application/problem+json", cancellationToken);

        return true;
    }
}
