namespace TmsSystem.Api.Responses;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; }
    public int Code { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ApiResponse<T> SuccessResponse(T? data, int code, string? message = null) => new()
    {
        Success = true,
        Code = code,
        Data = data,
        Message = message,
        Errors = Array.Empty<string>()
    };

    public static ApiResponse<T> FailureResponse(int code, string? message, IEnumerable<string>? errors = null, T? data = default) => new()
    {
        Success = false,
        Code = code,
        Data = data,
        Message = message,
        Errors = errors?.Where(error => !string.IsNullOrWhiteSpace(error)).ToArray() ?? Array.Empty<string>()
    };
}
