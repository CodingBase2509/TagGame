using Microsoft.Maui.Layouts;
using TagGame.Client.Ui.Components;
using TagGame.Client.Ui.ToastMessages;

namespace TagGame.Client.Ui.Services;

public class ToastService(IDispatcher dispatcher) : IToastService
{
    private ToastView? _toastView;

    public void Initialize(ToastView toastView)
    {
        _toastView = toastView;
    }
    
    public async Task ShowMessageAsync(string message, int duration = 3000)
    {
        await this.ShowAsync(ToastType.Info, message, duration);
    }

    public async Task ShowErrorAsync(string message, int duration = 3000)
    {
        await this.ShowAsync(ToastType.Error, message, duration);
    }

    public async Task ShowSuccessAsync(string message, int duration = 3000)
    {
        await this.ShowAsync(ToastType.Success, message, duration);
    }

    public async Task ShowAsync(ToastType type, string message, int duration = 3000)
    {
        try
        {
            if (_toastView is null)
                return;
            
            await dispatcher.DispatchAsync(async () => await _toastView.Show(message, type, duration));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}