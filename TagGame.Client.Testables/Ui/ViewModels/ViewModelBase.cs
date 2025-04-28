using CommunityToolkit.Mvvm.ComponentModel;
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
}