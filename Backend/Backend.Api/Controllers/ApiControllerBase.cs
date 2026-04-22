using Backend.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return result.GetType().IsGenericType ? Ok(GetValue(result)) : NoContent();
        }

        return ProcessFailure(result);
    }

    protected ActionResult HandleResult<TDto, TResponse>(
        Result<TDto> result,
        Func<TDto, TResponse> mapper)
    {
        if (result.IsSuccess)
        {
            return Ok(mapper(result.Value!));
        }

        return ProcessFailure(result);
    }

    private ActionResult ProcessFailure(Result result)
    {
        return result.FailureType switch
        {
            ResultFailureType.NotFound => NotFound(new { Error = result.Error }),
            ResultFailureType.ValidationFailure => BadRequest(new { Error = result.Error }),
            ResultFailureType.Conflict => Conflict(new { Error = result.Error }),
            ResultFailureType.Unauthorized => Unauthorized(new { Error = result.Error }),
            ResultFailureType.Forbidden => StatusCode(StatusCodes.Status403Forbidden, new { Error = result.Error }),
            ResultFailureType.NotImplemented => StatusCode(StatusCodes.Status501NotImplemented, new { Error = result.Error }),

            _ => StatusCode(StatusCodes.Status500InternalServerError, new { Error = result.Error ?? "Internal Server Error" })
        };
    }

    private static object? GetValue(Result result) =>
        result.GetType().GetProperty("Value")?.GetValue(result, null);
}