namespace TagGame.Client.Ui.ToastMessages;

public interface IToastService
{
    Task ShowMessageAsync(string message, int duration = 3000);
    
    Task ShowErrorAsync(string message, int duration = 3000);
    
    Task ShowSuccessAsync(string message, int duration = 3000);
    
    Task ShowAsync(ToastType type, string message, int duration = 3000);
}