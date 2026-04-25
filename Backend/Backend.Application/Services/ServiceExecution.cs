using Backend.Application.Common.Results;

namespace Backend.Application.Services;

internal static class ServiceExecution
{
    public static async Task<Result> ExecuteAsync(Func<Task<Result>> action, string errorMessage)
    {
        try
        {
            return await action();
        }
        catch
        {
            return Result.Failure(errorMessage, ResultFailureType.Unexpected);
        }
    }

    public static async Task<Result<T>> ExecuteAsync<T>(Func<Task<Result<T>>> action, string errorMessage)
    {
        try
        {
            return await action();
        }
        catch
        {
            return Result<T>.Failure(errorMessage, ResultFailureType.Unexpected);
        }
    }
}
