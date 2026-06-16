using TmsSystem.Application.Common;

namespace TmsSystem.BlazorWasm.ViewModels;

public sealed class PaginationState
{
    public const int DefaultPageSize = 10;
    public const int DefaultMaxPageButtons = 5;

    public PaginationState(int pageSize = DefaultPageSize, int maxPageButtons = DefaultMaxPageButtons)
    {
        PageSize = Math.Max(1, pageSize);
        MaxPageButtons = Math.Max(1, maxPageButtons);
    }

    public int PageSize { get; }
    public int MaxPageButtons { get; }
    public int CurrentPage { get; private set; } = 1;
    public int TotalItems { get; private set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalItems / (double)PageSize));
    public int StartIndex => TotalItems == 0 ? 0 : (CurrentPage - 1) * PageSize;
    public int FirstItemNumber => TotalItems == 0 ? 0 : StartIndex + 1;
    public int LastItemNumber => TotalItems == 0 ? 0 : Math.Min(StartIndex + PageSize, TotalItems);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public IReadOnlyList<int> PageNumbers
    {
        get
        {
            if (TotalItems == 0)
            {
                return Array.Empty<int>();
            }

            var visiblePageCount = Math.Min(MaxPageButtons, TotalPages);
            var startPage = Math.Max(1, CurrentPage - visiblePageCount / 2);
            var endPage = startPage + visiblePageCount - 1;

            if (endPage > TotalPages)
            {
                endPage = TotalPages;
                startPage = Math.Max(1, endPage - visiblePageCount + 1);
            }

            return Enumerable.Range(startPage, endPage - startPage + 1).ToArray();
        }
    }

    public void ApplyMetadata<T>(PagedResult<T> page)
    {
        TotalItems = Math.Max(0, page.TotalItems);
        CurrentPage = Math.Clamp(page.PageNumber, 1, TotalPages);
    }

    public void SetTotal(int totalItems)
    {
        TotalItems = Math.Max(0, totalItems);
        ClampCurrentPage();
    }

    public void Reset() => CurrentPage = 1;

    public void SetPage(int page)
    {
        CurrentPage = Math.Clamp(page, 1, TotalPages);
    }

    public void MovePrevious() => SetPage(CurrentPage - 1);

    public void MoveNext() => SetPage(CurrentPage + 1);

    public IReadOnlyList<T> GetItems<T>(IReadOnlyList<T> items)
    {
        SetTotal(items.Count);
        return TotalItems == 0
            ? Array.Empty<T>()
            : items.Skip(StartIndex).Take(PageSize).ToArray();
    }

    private void ClampCurrentPage()
    {
        CurrentPage = Math.Clamp(CurrentPage, 1, TotalPages);
    }
}