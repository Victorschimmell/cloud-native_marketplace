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
            ResultFailureType.NotFound => Problem(detail: result.Error, statusCode: StatusCodes.Status404NotFound, title: "Not Found"),
            ResultFailureType.ValidationFailure => Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest, title: "Bad Request"),
            ResultFailureType.Conflict => Problem(detail: result.Error, statusCode: StatusCodes.Status409Conflict, title: "Conflict"),
            ResultFailureType.Unauthorized => Problem(detail: result.Error, statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized"),
            ResultFailureType.Forbidden => Problem(detail: result.Error, statusCode: StatusCodes.Status403Forbidden, title: "Forbidden"),
            ResultFailureType.NotImplemented => Problem(detail: result.Error, statusCode: StatusCodes.Status501NotImplemented, title: "Not Implemented"),

            _ => Problem(detail: result.Error ?? "An unexpected error occurred.", statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error")
        };
    }

    private static object? GetValue(Result result) =>
        result.GetType().GetProperty("Value")?.GetValue(result, null);
}