using TagGame.Client.Core.Services.Abstractions;
using Connection = Microsoft.Maui.Networking.Connectivity;

namespace TagGame.Client.Infrastructure.Connectivity;

/// <summary>
/// MAUI-based network connectivity monitor.
/// Provides current online state, a debounced change event, and a one-shot wait for online.
/// </summary>
public sealed class NetworkConnectivity : INetworkConnectivity, IDisposable
{
    private CancellationTokenSource? _debounceCts;
    private volatile bool _isOnline;

    /// <inheritdoc />
    public bool IsOnline => _isOnline;
    /// <inheritdoc />
    public event EventHandler<bool>? OnlineChanged;

    public NetworkConnectivity()
    {
        _isOnline = Map(Connection.NetworkAccess);
        Connection.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <inheritdoc />
    public Task WaitForOnlineAsync(CancellationToken ct = default)
    {
        if (IsOnline)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        OnlineChanged += OnlineChangedCallback;

        if (IsOnline)
        {
            OnlineChanged -= OnlineChangedCallback;
            tcs.TrySetResult();
            return tcs.Task;
        }

        if (!ct.CanBeCanceled)
            return tcs.Task;

        ct.Register(() =>
        {
            OnlineChanged -= OnlineChangedCallback;
            tcs.TrySetCanceled(ct);
        });
        return tcs.Task;

        void OnlineChangedCallback(object? _, bool online)
        {
            if (!online)
                return;

            OnlineChanged -= OnlineChangedCallback;
            tcs.TrySetResult();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        Connection.ConnectivityChanged -= OnConnectivityChanged;
        _debounceCts?.Dispose();
    }

    private async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        try
        {
            if (_debounceCts is not null)
                await _debounceCts.CancelAsync();

            _debounceCts = new CancellationTokenSource();
            await Task.Delay(250, _debounceCts.Token);
        }
        catch
        {
            return;
        }

        var isOnline = Map(e.NetworkAccess);
        if (isOnline == _isOnline)
            return;

        _isOnline = isOnline;
        OnlineChanged?.Invoke(this, isOnline);
    }

    private static bool Map(NetworkAccess access) =>
        access is NetworkAccess.Internet or NetworkAccess.ConstrainedInternet;
}
