public class LoadingService
{
    public event Action? OnChange;

    public bool IsLoading { get; private set; }
    public string Message { get; private set; } = "Loading...";
    public bool CanCancel { get; private set; }

    private CancellationTokenSource? _cts;
    public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

    public void Show(
        string message = "Loading...",
        bool canCancel = false)
    {
        _cts?.Dispose();

        IsLoading = true;
        Message = message;
        CanCancel = canCancel;
        _cts = new CancellationTokenSource();

        OnChange?.Invoke();
    }

    public void Hide()
    {
        IsLoading = false;
        Message = string.Empty;
        CanCancel = false;

        _cts?.Dispose();
        _cts = null;

        OnChange?.Invoke();
    }

    public void Cancel()
    {
        _cts?.Cancel();
        Hide();
    }
}