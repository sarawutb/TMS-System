namespace TmsSystem.BlazorWasm.Models;

public sealed record ApiResponse<T>
{
    public bool Success { get; init; }
    public int Code { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = new();
}
