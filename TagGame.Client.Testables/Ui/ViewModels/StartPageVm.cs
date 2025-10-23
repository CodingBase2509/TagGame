using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Client.Ui.ToastMessages;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.DTOs.Games;
using INavigation = TagGame.Client.Ui.Navigation.INavigation;
using NavigationMode = TagGame.Client.Ui.Navigation.NavigationMode;

namespace TagGame.Client.Ui.ViewModels;

public partial class StartPageVm(
    RestClient api,
    ConfigHandler config,
    Localization loc,
    INavigation nav,
    IToastService toast) : ViewModelBase
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
    
    public override async Task InitializeAsync()
    {
        if (config.Exists<RoomConfig>())
        {
            await HandleOpenGame();
        }
        
        // load user name
        var userConfig = await config.ReadAsync<UserConfig>();
        if (userConfig is null)
            return;
        
        var greeting = loc.Get("greeting", "StartPage");
        Greeting = $"{greeting}, {userConfig.Username}!";
        _userConfig = userConfig;
    }

    private async Task HandleOpenGame()
    {
        var roomConfig = await config.ReadAsync<RoomConfig>();
        var response = await api.GetRoomAsync(roomConfig!.RoomId);
        var room = response.Value;

        if (!response.IsSuccess)
        {
            config.Delete<RoomConfig>();
            return;
        }

        const string page = "StartPage";
        var joinGame = await Shell.Current.DisplayAlert(
            loc.Get("open-game-title", page),
            loc.Get("open-game-text", page),
            loc.Get("yes", page),
            loc.Get("no", page));

        if (!joinGame)
        {
            config.Delete<RoomConfig>();
            return;
        }

        switch (room.State)
        { 
            case GameState.Lobby:
                nav.GoToLobby(NavigationMode.Forward, new()
                {
                    { "roomName", room.Name },
                    { "accessCode", room.AccessCode }
                }); 
                break;
            case GameState.Preperation: 
                // go to game
                break;
            case GameState.InGame: 
                await Shell.Current.DisplayAlert(
                    loc.Get("attention", page),
                    loc.Get("room-ingame", page), 
                    "OK");
                break;
            default:
                break;
            }
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
        {
            await toast.ShowErrorAsync(response.Error.Message);
            return;
        }
        
        await nav.GoToLobby(NavigationMode.Forward, new Dictionary<string, object>(){
            { "roomId", response.Value.RoomId },
            { "roomName", response.Value.RoomName },
            { "accessCode", response.Value.AccessCode },
            { "playerId", response.Value.PlayerId }
        });
    }

    [RelayCommand]
    private async Task JoinRoomAsync()
    {
        var request = new JoinGameRoom.JoinGameRoomRequest()
        {
            UserId = _userConfig?.UserId ?? Guid.Empty,
            GameName = ExistingGameRoomName,
            AccessCode = AccessCode
        };
        
        var response = await api.JoinRoomAsync(request);
        if (!response.IsSuccess || response.Value is null)
        {
            await toast.ShowErrorAsync(response.Error.Message);
            return;
        }

        await nav.GoToLobby(NavigationMode.Forward, new Dictionary<string, object>()
        {
            { "roomId", response.Value.Room.Id },
            { "roomName", response.Value.Room.Name },
            { "accessCode", response.Value.Room.AccessCode },
            { "playerId", response.Value.PlayerId }
        });
    }
}