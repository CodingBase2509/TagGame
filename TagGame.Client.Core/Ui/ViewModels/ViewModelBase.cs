using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Ui.ViewModels;

public abstract partial class ViewModelBase : ObservableObject, IDisposable
{
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private CancellationTokenSource _lifetimeCts = new();
    private AsyncRelayCommand? _refreshCommand;
    private int _busyOperations;
    private bool _isDisposed;

    [ObservableProperty]
    private bool _isInitialized;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    private IUiDispatcher Dispatcher => SpUtils.GetRequiredService<IUiDispatcher>();

    protected IToastPublisher Toast => SpUtils.GetRequiredService<IToastPublisher>();

    protected INavigationService Navigation => SpUtils.GetRequiredService<INavigationService>();

    protected CancellationToken ViewModelCancellationToken => _lifetimeCts.Token;

    public IAsyncRelayCommand RefreshCommand => _refreshCommand ??= new AsyncRelayCommand(ExecuteRefreshAsync, CanExecuteRefresh);

    public virtual Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task OnAppearingAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public virtual Task OnDisappearingAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    protected virtual Task RefreshAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (IsInitialized)
            return;

        await _initializationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (IsInitialized)
                return;

            await RunBusyAsync(InitializeAsync, cancellationToken).ConfigureAwait(false);
            IsInitialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    protected Task RunBusyAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RunBusyAsync(_ => action(), cancellationToken);
    }

    protected Task<TResult> RunBusyAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RunBusyAsync(_ => action(), cancellationToken);
    }

    protected async Task RunBusyAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfDisposed();

        var (token, linkedCts) = CreateScopedToken(cancellationToken);
        BeginBusy();
        try
        {
            await action(token).ConfigureAwait(false);
        }
        finally
        {
            linkedCts?.Dispose();
            EndBusy();
        }
    }

    protected async Task<TResult> RunBusyAsync<TResult>(Func<CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfDisposed();

        var (token, linkedCts) = CreateScopedToken(cancellationToken);
        BeginBusy();
        try
        {
            return await action(token).ConfigureAwait(false);
        }
        finally
        {
            linkedCts?.Dispose();
            EndBusy();
        }
    }

    protected Task RunRefreshAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RunRefreshAsync(_ => action(), cancellationToken);
    }

    protected async Task RunRefreshAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ThrowIfDisposed();

        if (IsRefreshing)
            return;

        var (token, linkedCts) = CreateScopedToken(cancellationToken);
        IsRefreshing = true;
        try
        {
            await action(token).ConfigureAwait(false);
        }
        finally
        {
            linkedCts?.Dispose();
            IsRefreshing = false;
        }
    }

    protected Task RunOnUiThreadAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.OnMainThreadAsync(action);
    }

    protected Task RunOnUiThreadAsync(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Dispatcher.OnMainThreadAsync(action);
    }

    protected void CancelPendingOperations()
    {
        if (_isDisposed)
            return;

        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
        _lifetimeCts = new CancellationTokenSource();
    }

    public virtual void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
        _initializationLock.Dispose();
        GC.SuppressFinalize(this);
    }

    private Task ExecuteRefreshAsync() => RunRefreshAsync(RefreshAsync, CancellationToken.None);

    private bool CanExecuteRefresh() => !IsRefreshing;

    private (CancellationToken Token, CancellationTokenSource? LinkedCts) CreateScopedToken(CancellationToken cancellationToken)
    {
        if (!cancellationToken.CanBeCanceled)
            return (ViewModelCancellationToken, null);

        var linked = CancellationTokenSource.CreateLinkedTokenSource(ViewModelCancellationToken, cancellationToken);
        return (linked.Token, linked);
    }

    private void BeginBusy()
    {
        var count = Interlocked.Increment(ref _busyOperations);
        if (count == 1)
            IsBusy = true;
    }

    private void EndBusy()
    {
        var count = Interlocked.Decrement(ref _busyOperations);
        if (count <= 0)
        {
            Interlocked.Exchange(ref _busyOperations, 0);
            IsBusy = false;
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_isDisposed, this);

    partial void OnIsRefreshingChanged(bool value) => _refreshCommand?.NotifyCanExecuteChanged();
}
