namespace TmsSystem.Application.Common;

public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages => TotalItems == 0 ? 1 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 10) => new()
    {
        Items = Array.Empty<T>(),
        PageNumber = Math.Max(1, pageNumber),
        PageSize = Math.Max(1, pageSize),
        TotalItems = 0
    };

    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalItems, int pageNumber, int pageSize) => new()
    {
        Items = items,
        PageNumber = Math.Max(1, pageNumber),
        PageSize = Math.Max(1, pageSize),
        TotalItems = Math.Max(0, totalItems)
    };

    public static PagedResult<T> Create(IReadOnlyList<T> source, int pageNumber, int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, pageSize);
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
        return Create(items, source.Count, pageNumber, pageSize);
    }
}