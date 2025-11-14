using System.Diagnostics;
using TagGame.Client.Core.Notifications;

namespace TagGame.Client.Ui.Components.Toasts;

internal sealed class ToastEntry : IDisposable
{
    private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private CancellationTokenSource? _timerCts;
    private Stopwatch? _stopwatch;

    public ToastEntry(ToastRequest request, Toast view)
    {
        ToastRequest = request;
        ToastView = view;
        RemainingMs = Math.Max(0, ToastRequest.DurationMs);
    }

    public ToastRequest ToastRequest { get; init; }
    public Toast ToastView { get; init; }
    public int RemainingMs { get; set; }

    public Task Completion => _completion.Task;

    public void Complete() => _completion.TrySetResult();

    public CancellationToken StartTimer()
    {
        CancelTimer();

        if (RemainingMs <= 0)
        {
            _completion.TrySetResult();
            return CancellationToken.None;
        }

        _stopwatch = Stopwatch.StartNew();
        _timerCts = new CancellationTokenSource();
        return _timerCts.Token;
    }

    public void PauseTimer()
    {
        if (_stopwatch is null || _timerCts is null)
            return;

        _stopwatch.Stop();
        var elapsed = (int)_stopwatch.ElapsedMilliseconds;
        RemainingMs = Math.Max(0, RemainingMs - elapsed);

        CancelTimer();
    }

    private void CancelTimer()
    {
        if (_timerCts is null)
            return;

        try
        {
            if (!_timerCts.IsCancellationRequested)
                _timerCts.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // ignore
        }
        finally
        {
            _timerCts.Dispose();
            _timerCts = null;
            _stopwatch = null;
        }
    }

    public void Dispose() => CancelTimer();
}
