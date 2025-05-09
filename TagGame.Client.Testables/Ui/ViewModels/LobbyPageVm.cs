using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Client.Ui.ViewModels;

[QueryProperty(nameof(RoomName), "roomName")]
[QueryProperty(nameof(AccessCode), "accessCode")]
public partial class LobbyPageVm(LobbyClient lobby, ConfigHandler config, INavigation nav) : ViewModelBase
{
    [ObservableProperty]
    private string _roomName = string.Empty;
    
    [ObservableProperty]
    private string _accessCode = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<Player> _players = [];

    [ObservableProperty]
    public Guid? _roomOwnerId;
    
    private GameRoom? _room;
    
    public override async Task InitializeAsync()
    {
        await lobby.InitializeAsync();
        
        lobby.SetupReceiveGameRoomInfo(async room =>
        {
            await OnMainThreadAsync(() =>
            {
                this._room = room;
                RoomOwnerId = room.OwnerUserId;
                RoomName = room.Name;
                AccessCode = room.AccessCode;

                foreach (var player in room.Players
                             .Where(player => !Players.Any(p => Equals(p.Id, player.Id))))
                    Players.Add(player);
            });

            await config.WriteAsync(new RoomConfig()
            {
                RoomId = room.Id,
                RoomName = room.Name,
                AccessCode = room.AccessCode,
                State = room.State,
            });
        });
        
        lobby.SetupReceiveGameSettingsUpdated(settings =>
        {
            if (_room is null)
                return Task.CompletedTask; 
            
            _room.Settings = settings;

            foreach (var seekerId in settings.SeekerIds)
            {
                var player = Players.FirstOrDefault(p => Equals(p.Id, seekerId));
                if (player is null)
                    continue;
                
                player.Type = PlayerType.Seeker;
                OnPropertyChanged(nameof(Players));
            }
            
            return Task.CompletedTask;
        });
        
        lobby.SetupReceivePlayerJoined(async player =>
        {
            if (_room is null)
                return;

            await OnMainThreadAsync(() =>
            {
                Players.Add(player);
                _room.Players.Add(player);
            });
        });
        
        lobby.SetupReceivePlayerLeft(async info =>
        { 
            var player = Players.FirstOrDefault(p => Equals(p.Id, info.Player.Id));
            
            switch (info.DisconnectType)
            {
                case PlayerDisconnectType.LeftGame when player is null || _room is null:
                case PlayerDisconnectType.LeftWithReconnect when player is null || _room is null:
                default:
                    return;
                case PlayerDisconnectType.LeftGame:
                    await OnMainThreadAsync(() =>
                    {
                        Players.Remove(player);
                        _room.Players.Remove(player);
                    });
                    break;
                case PlayerDisconnectType.LeftWithReconnect:
                    await OnMainThreadAsync(() =>
                    {
                        player.ConnectionId = info.Player.ConnectionId;
                    });
                    break;
            }
        });

        await lobby.ConnectAsync();
    }

    public override Task CleanUpAsync()
    {
        Players.Clear();
        return base.CleanUpAsync();
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        var deleteSuccess = config.Delete<RoomConfig>();
        
        await lobby.DisconnectAsync();
        await nav.GoToStart(NavigationMode.Backward);
    }

    [RelayCommand]
    private async Task OpenSettingsPageAsync()
    {
        await Shell.Current.DisplayAlert("Button Click", "Settings", "OK");
    }

    [RelayCommand]
    private async Task UpdatePlayerTypeAsync(Player player)
    {
        if (_room is null)
            return;
        
        var seekerIds = _room.Settings.SeekerIds;
        switch (player.Type)
        {
            case PlayerType.Hider:
                if (!seekerIds.Contains(player.Id))
                    return;
                seekerIds.Remove(player.Id);
                break;
            case PlayerType.Seeker:
                if (seekerIds.Contains(player.Id))
                    return;
                seekerIds.Add(player.Id);
                break;
            default:
                return;
        }
        
        await lobby.UpdateGameSettingsAsync(_room.Settings);
    }

    [RelayCommand]
    private async Task StartGameAsync()
    {
        await Shell.Current.DisplayAlert("Button Click", "Start Game", "OK");
    }

    [RelayCommand]
    private async Task CopyElement(string text)
    {
        await OnMainThreadAsync(async () => await Clipboard.Default.SetTextAsync(text));
    }
}