namespace UsersApi.Application.Common;

public enum ErrorType
{
    None,
    NotFound,
    Conflict
}

public class Result<T>
{
    public T? Value { get; private init; }
    public ErrorType Error { get; private init; }
    public string? Message { get; private init; }
    public bool IsSuccess => Error == ErrorType.None;

    public static Result<T> Ok(T value) => new() { Value = value };

    public static Result<T> NotFound(string message) =>
        new() { Error = ErrorType.NotFound, Message = message };

    public static Result<T> Conflict(string message) =>
        new() { Error = ErrorType.Conflict, Message = message };
}