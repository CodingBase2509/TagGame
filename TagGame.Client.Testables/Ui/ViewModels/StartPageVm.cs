using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Client.Ui.ViewModels;

public partial class StartPageVm(RestClient api, INavigation nav) : ViewModelBase
{
    [ObservableProperty]
    private string _newGameRoomName = string.Empty;

    [ObservableProperty]
    private string _existingGameRoomName = string.Empty;
    
    [ObservableProperty] 
    private string _accessCode = string.Empty;

    [RelayCommand]
    private async Task CreateNewRoomAsync()
    {
        var request = new CreateGameRoom.CreateGameRoomRequest()
        {
            UserId = Guid.NewGuid(),
            GameRoomName = NewGameRoomName,
        };
        
        var response = await api.CreateRoomAsync(request);
        if (!response.IsSuccess || response.Value is null)
            return;
        
        await nav.GoToLobby(NavigationMode.Forward, new Dictionary<string, object>(){
            { "roomId", response.Value.RoomId },
            { "roomName", response.Value.RoomName },
            { "accessCode", response.Value.AccessCode }
        });
    }

    [RelayCommand]
    public async Task JoinRoomAsync()
    {
        
    }
}