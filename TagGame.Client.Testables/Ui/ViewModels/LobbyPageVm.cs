using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace TagGame.Client.Ui.ViewModels;

[QueryProperty(nameof(RoomId), "roomId")]
[QueryProperty(nameof(RoomName), "roomName")]
[QueryProperty(nameof(AccessCode), "accessCode")]
public partial class LobbyPageVm : ViewModelBase
{
    [ObservableProperty]
    private Guid roomId = Guid.Empty; 
    
    [ObservableProperty]
    private string roomName = string.Empty;
    
    [ObservableProperty]
    private string accessCode = string.Empty;

    public override async Task InitializeAsync()
    {
        
    }

    [RelayCommand]
    public async Task GoBackAsync()
    {
        
    }

    [RelayCommand]
    public async Task OpenSettingsPageAsync()
    {
        
    }

    [RelayCommand]
    public async Task CopyElement(string text)
    {
        var disp = Dispatcher.GetForCurrentThread();
        if (disp is not null && disp.IsDispatchRequired)
        {
            await disp.DispatchAsync(async () => await Copy(text));
        }
        else
        {
            await Copy(text);
        }
    }

    private async Task Copy(string text)
    {
        await Clipboard.Default.SetTextAsync(text);
    }
}