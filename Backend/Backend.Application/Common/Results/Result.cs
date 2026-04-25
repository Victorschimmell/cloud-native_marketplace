namespace Backend.Application.Common.Results;

public enum ResultFailureType
{
    ValidationFailure = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Unexpected = 6,
    NotImplemented = 7
}

public class Result
{
    protected Result(bool isSuccess, string? error, ResultFailureType? failureType)
    {
        IsSuccess = isSuccess;
        Error = error;
        FailureType = failureType;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public ResultFailureType? FailureType { get; }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string error, ResultFailureType failureType = ResultFailureType.Unexpected) => new(false, error, failureType);

    public static Result ValidationFailure(string error) => Failure(error, ResultFailureType.ValidationFailure);

    public static Result NotFound(string error) => Failure(error, ResultFailureType.NotFound);

    public static Result Conflict(string error) => Failure(error, ResultFailureType.Conflict);

    public static Result Unauthorized(string error) => Failure(error, ResultFailureType.Unauthorized);

    public static Result NotImplemented(string error = "This service is not implemented yet.") => Failure(error, ResultFailureType.NotImplemented);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, string? error, ResultFailureType? failureType)
        : base(isSuccess, error, failureType)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static new Result<T> Failure(string error, ResultFailureType failureType = ResultFailureType.Unexpected) => new(false, default, error, failureType);

    public static new Result<T> ValidationFailure(string error) => Failure(error, ResultFailureType.ValidationFailure);

    public static new Result<T> NotFound(string error) => Failure(error, ResultFailureType.NotFound);

    public static new Result<T> Conflict(string error) => Failure(error, ResultFailureType.Conflict);

    public static new Result<T> Unauthorized(string error) => Failure(error, ResultFailureType.Unauthorized);

    public static new Result<T> NotImplemented(string error = "This service is not implemented yet.") => Failure(error, ResultFailureType.NotImplemented);
}
