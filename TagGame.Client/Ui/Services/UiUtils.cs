namespace TagGame.Client.Ui.Services;

public static class UiUtils
{
    public static Task OnMainThreadAsync(Func<Task> action) =>
        MainThread.IsMainThread ? action() : MainThread.InvokeOnMainThreadAsync(action);

    public static Task OnMainThreadAsync(Action action) => OnMainThreadAsync(() => { action(); return Task.CompletedTask; });
}
