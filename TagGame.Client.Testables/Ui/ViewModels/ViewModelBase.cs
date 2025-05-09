using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Dispatching;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isBusy;

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public virtual Task CleanUpAsync()
    {
        return Task.CompletedTask;
    }

    protected async Task OnMainThreadAsync(Action action)
    {
        var disp = Dispatcher.GetForCurrentThread();
        if (disp is not null && disp.IsDispatchRequired)
        {
            await disp.DispatchAsync(action);
        }
        else
        {
            action();
        }
    }

    protected async Task OnMainThreadAsync(Func<Task> action)
    {
        var disp = Dispatcher.GetForCurrentThread();
        if (disp is not null && disp.IsDispatchRequired)
        {
            await disp.DispatchAsync(async () => await action());
        }
        else
        {
            await action();
        }
    }
}