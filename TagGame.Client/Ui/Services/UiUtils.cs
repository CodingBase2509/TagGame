using TagGame.Client.Core.Services;

namespace TagGame.Client.Ui.Services;

public class UiUtils : IUiDispatcher
{
    public Task OnMainThreadAsync(Func<Task> action) =>
        MainThread.IsMainThread ? action() : MainThread.InvokeOnMainThreadAsync(action);

    public Task OnMainThreadAsync(Action action) => OnMainThreadAsync(() => { action(); return Task.CompletedTask; });
}
