using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls;

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

    public async Task InitializeAsync()
    {
        
    }
}