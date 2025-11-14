using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Ui.Services;

namespace TagGame.Client.Ui.Components.Toasts;

public partial class ToastHost : ContentView, IDisposable
{
    private Toast? _currentView;
    private readonly Queue<ToastRequest> _pending = new();
    private CancellationTokenSource? _currentCts;
    private readonly ILocalizer _loc;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ToastHost(ILocalizer localizer)
    {
        _loc = localizer;
        InitializeComponent();
    }

    public async Task ShowAsync(ToastRequest req, CancellationToken ct = default)
    {
        // Priority: High/Critical preempts current immediately
        if (ToastHostService.IsPriority(req) && _currentView is not null)
        {
            await _gate.WaitAsync(ct);
            try
            {
                await (_currentCts?.CancelAsync() ?? Task.CompletedTask);
            }
            finally
            {
                _gate.Release();
            }
            _ = MainThread.InvokeOnMainThreadAsync(async () => await ReplaceCurrentAsync(req));
            return;
        }

        // If a toast is currently shown (or animating), queue and show count badge
        if (_currentView is not null)
        {
            int count;
            await _gate.WaitAsync(ct);
            try
            {
                _pending.Enqueue(req);
                count = _pending.Count;
            }
            finally
            {
                _gate.Release();
            }
            _ = MainThread.InvokeOnMainThreadAsync(() => UpdateBadge(count));
            return;
        }

        // No current toast → show now and then drain queue
        _ = MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await ShowNowAsync(req, ct);
            while (true)
            {
                ToastRequest next;
                await _gate.WaitAsync(ct);
                try
                {
                    if (_pending.Count == 0)
                        break;

                    next = _pending.Dequeue();
                }
                finally
                {
                    _gate.Release();
                }

                await ShowNowAsync(next, ct);
                int pendingCount;

                await _gate.WaitAsync(ct);
                try
                {
                    pendingCount = _pending.Count - 1;
                }
                finally
                {
                    _gate.Release();
                }
                UpdateBadge(pendingCount);
            }
        });
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _gate.Dispose();
    }

    private async Task ShowNowAsync(ToastRequest req, CancellationToken externalCt)
    {
        var toast = BuildToast(req);
        AddToastToContainer(toast);
        await ToastHostService.AnimateInAsync(toast);
        await RunToastAsync(toast, req.DurationMs, externalCt);
    }

    private async Task ReplaceCurrentAsync(ToastRequest req)
    {
        var old = _currentView;
        if (old is null)
        {
            await ShowNowAsync(req, CancellationToken.None);
            return;
        }

        // Animate old out quickly
        await Task.WhenAll(
            old.TranslateToAsync(0, -24, 120, Easing.CubicIn),
            old.FadeToAsync(0, 100, Easing.CubicIn));
        if (ToastContainer.Children.Contains(old))
            ToastContainer.Children.Remove(old);

        var fresh = BuildToast(req);
        AddToastToContainer(fresh);

        await Task.WhenAll(
            fresh.FadeToAsync(1, 140, Easing.CubicOut),
            fresh.TranslateToAsync(0, 0, 160, Easing.CubicOut));
        await RunToastAsync(fresh, req.DurationMs, CancellationToken.None);
    }

    private Toast BuildToast(ToastRequest req)
    {
        var toast = new Toast
        {
            Text = ToastHostService.ResolveText(req, _loc),
            Type = req.Type,
            Opacity = 0,
            TranslationY = -24
        };
        toast.Apply();
        ToastHostService.PrepareToast(toast);
        return toast;
    }

    private void AddToastToContainer(Toast toast)
    {
        ToastContainer.Children.Add(toast);
        IsVisible = true;
    }

    private async Task RunToastAsync(Toast toast, int durationMs, CancellationToken externalCt)
    {
        _currentView = toast;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
        await _gate.WaitAsync(linked.Token);
        try { _currentCts = linked; }
        finally { _gate.Release(); }

        try
        {
            await ToastHostService.WaitDurationAsync(Dispatcher, durationMs, linked.Token);
        }
        catch (OperationCanceledException) { }

        if (ReferenceEquals(_currentView, toast))
        {
            await ToastHostService.AnimateOutAsync(toast);
            if (ToastContainer.Children.Contains(toast))
                ToastContainer.Children.Remove(toast);

            _currentView = null;
            int pendingCount;
            await _gate.WaitAsync(linked.Token);
            try
            {
                pendingCount = _pending.Count - 1;
            }
            finally
            {
                _gate.Release();
            }
            IsVisible = _currentView is not null || pendingCount > 0;
            UpdateBadge(pendingCount);
        }
    }

    private void UpdateBadge(int count)
    {
        Badge.IsVisible = count > 0;
        if (count > 0)
            BadgeLabel.Text = ToastHostService.FormatBadgeCount(count);
    }
}
