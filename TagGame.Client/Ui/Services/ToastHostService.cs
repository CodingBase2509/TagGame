using TagGame.Client.Core.Localization;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Ui.Components.Toasts;

namespace TagGame.Client.Ui.Services;

/// <summary>
/// Helper service for ToastHost: text resolution, animations, timers, and small helpers.
/// Keeps ToastHost.xaml.cs lean and focused on orchestration.
/// </summary>
public static class ToastHostService
{
    public static string ResolveText(ToastRequest req, ILocalizer loc) =>
        req.IsLocalized ? loc.GetString(req.Message) : req.Message;

    public static void PrepareToast(Toast toast)
    {
        toast.HorizontalOptions = LayoutOptions.Center;
        toast.VerticalOptions = LayoutOptions.Start;
        toast.Opacity = 0;
    }

    public static Task AnimateInAsync(View view)
    {
        return Task.WhenAll(
            view.FadeToAsync(1, 90, Easing.CubicOut),
            view.TranslateToAsync(0, 10, 350, Easing.CubicOut)
        );
    }

    public static Task AnimateOutAsync(View view)
    {
        return Task.WhenAll(
            view.TranslateToAsync(0, -20, 200, Easing.CubicIn),
            view.FadeToAsync(0, 250, Easing.CubicIn)
        );
    }

    public static Task WaitDurationAsync(IDispatcher dispatcher, int durationMs, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (durationMs <= 0)
        {
            tcs.SetResult(null);
            return tcs.Task;
        }
        if (ct.IsCancellationRequested)
        {
            tcs.TrySetCanceled(ct);
            return tcs.Task;
        }

        dispatcher.StartTimer(TimeSpan.FromMilliseconds(durationMs), () =>
        {
            if (ct.IsCancellationRequested)
                tcs.TrySetCanceled(ct);
            else
                tcs.TrySetResult(null);
            return false; // one-shot
        });

        ct.Register(() => tcs.TrySetCanceled(ct));
        return tcs.Task;
    }

    public static bool IsPriority(ToastRequest req) => req.Priority is ToastPriority.High or ToastPriority.Critical;

    public static string FormatBadgeCount(int count) => count > 9 ? "9+" : count.ToString();
}
