namespace TaskMaster.Application.Common.Models;

public interface IResult
{
    bool IsSuccess { get; }
    string? ErrorCode { get; }
    string? ErrorMessage { get; }
}

public class Result<T> : IResult
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, T? value, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public static Result<T> Failure(string errorCode, string errorMessage) =>
        new(false, default, errorCode, errorMessage);

    public Result<U> ToFailure<U>()
    {
        if (IsSuccess)
            throw new InvalidOperationException("Can't convert a successful result to a failure.");
        return Result<U>.Failure(ErrorCode!, ErrorMessage!);
    }
}

public class Result : IResult
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(bool isSuccess, string? errorCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string errorCode, string errorMessage) =>
        new(false, errorCode, errorMessage);
}
