namespace BlazorApp.Client.Services.Base;

public class LoadingService
{
    private int _activeRequests = 0;
    public event Action<bool>? OnLoadingStateChanged;

    public void StartLoading()
    {
        _activeRequests++;
        if (_activeRequests == 1)
        {
            OnLoadingStateChanged?.Invoke(true);
        }
    }

    public void StopLoading()
    {
        _activeRequests = Math.Max(0, _activeRequests - 1);
        if (_activeRequests == 0)
        {
            OnLoadingStateChanged?.Invoke(false);
        }
    }

    public bool IsLoading => _activeRequests > 0;
}
