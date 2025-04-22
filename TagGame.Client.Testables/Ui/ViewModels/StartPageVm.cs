using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Shared.DTOs.Games;

namespace TagGame.Client.Ui.ViewModels;

public partial class StartPageVm(RestClient api, ConfigHandler config, Localization loc, INavigation nav) : ViewModelBase
{
    [ObservableProperty]
    private string _newGameRoomName = string.Empty;

    [ObservableProperty]
    private string _existingGameRoomName = string.Empty;
    
    [ObservableProperty] 
    private string _accessCode = string.Empty;

    [ObservableProperty]
    private string _greeting = string.Empty;
    
    private UserConfig? _userConfig;
    
    public async Task InitializeAsync()
    {
        var userConfig = await config.ReadAsync<UserConfig>();
        if (userConfig is null)
            return;
        
        var greting = loc.Get("greeting", "StartPage");
        Greeting = $"{greting}, {userConfig.Username}!";
        _userConfig = userConfig;
    }
    
    [RelayCommand]
    private async Task CreateNewRoomAsync()
    {
        var request = new CreateGameRoom.CreateGameRoomRequest()
        {
            UserId = _userConfig?.UserId ?? Guid.Empty,
            GameRoomName = NewGameRoomName,
        };
        
        var response = await api.CreateRoomAsync(request);
        if (!response.IsSuccess || response.Value is null)
            return;
        
        await nav.GoToLobby(NavigationMode.Parallel, new Dictionary<string, object>(){
            { "roomId", response.Value.RoomId },
            { "roomName", response.Value.RoomName },
            { "accessCode", response.Value.AccessCode },
            { "playerId", response.Value.PlayerId }
        });
    }

    [RelayCommand]
    public async Task JoinRoomAsync()
    {
        var request = new JoinGameRoom.JoinGameRoomRequest()
        {
            UserId = _userConfig?.UserId ?? Guid.Empty,
            GameName = ExistingGameRoomName,
            AccessCode = AccessCode
        };
        
        var response = await api.JoinRoomAsync(request);
        if (!response.IsSuccess || response.Value is null)
            return;

        await nav.GoToLobby(NavigationMode.Parallel, new Dictionary<string, object>()
        {
            { "roomId", response.Value.Room.Id },
            { "roomName", response.Value.Room.Name },
            { "accessCode", response.Value.Room.AccessCode },
            { "playerId", response.Value.PlayerId }
        });
    }
}