namespace TmsSystem.Application.Common;

public sealed class OperationResult<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public static OperationResult<T> Success(T data, string message = "Success.") => new() { IsSuccess = true, Data = data, Message = message };
    public static OperationResult<T> Failure(string message, params string[] errors) => new() { IsSuccess = false, Message = message, Errors = errors };
}
