using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Controls;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Client.Ui.ToastMessages;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Client.Ui.ViewModels;

[QueryProperty(nameof(RoomName), "roomName")]
[QueryProperty(nameof(AccessCode), "accessCode")]
public partial class LobbyPageVm(
    LobbyClient lobby, 
    ConfigHandler config, 
    INavigation nav,
    IToastService toast) : ViewModelBase
{
    [ObservableProperty]
    private string _roomName = string.Empty;
    
    [ObservableProperty]
    private string _accessCode = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<Player> _players = [];

    [ObservableProperty]
    private Guid? _roomOwnerId;
    
    public bool UserIsRoomOwner => Equals(_roomOwnerId, currentUserId);
    
    private GameRoom? _room;
    private Guid? currentUserId;
    
    public override async Task InitializeAsync()
    {
        await lobby.InitializeAsync();
        
        var user = await config.ReadAsync<UserConfig>();
        currentUserId = user.UserId;
        OnPropertyChanged(nameof(UserIsRoomOwner));
        
        lobby.SetupReceiveGameRoomInfo(async room =>
        {
            await OnMainThreadAsync(() =>
            {
                this._room = room;
                RoomOwnerId = room.OwnerUserId;
                RoomName = room.Name;
                AccessCode = room.AccessCode;
                OnPropertyChanged(nameof(UserIsRoomOwner));

                foreach (var player in room.Players
                             .Where(player => !Players.Any(p => Equals(p.Id, player.Id))))
                {
                    if (_room.Settings.SeekerIds.Contains(player.Id))
                        player.Type = PlayerType.Seeker;
                    Players.Add(player);
                }
            });

            await config.WriteAsync(new RoomConfig()
            {
                RoomId = room.Id,
                RoomName = room.Name,
                AccessCode = room.AccessCode,
                State = room.State,
            });
        });
        
        lobby.SetupReceiveGameSettingsUpdated(async settings =>
        {
            if (_room is null)
                return; 
            
            _room.Settings = settings;

            foreach (var player in _room.Players)
            {
                if (settings.SeekerIds.Contains(player.Id) &&
                    player.Type != PlayerType.Seeker)
                    UpdatePlayer(player, PlayerType.Seeker);

                if (settings.SeekerIds.Contains(player.Id) ||
                    player.Type == PlayerType.Hider) 
                    continue;
                
                UpdatePlayer(player, PlayerType.Hider);
            }

            await toast.ShowMessageAsync("update-settings");
            return;

            void UpdatePlayer(Player player, PlayerType newType)
            {
                player.Type = newType;
                var index = Players.IndexOf(player);
                Players.RemoveAt(index);
                Players.Insert(index, player);
            }
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
            
            await toast.ShowMessageAsync("player-joined");
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
            
            await toast.ShowMessageAsync("player-left");
        });

        lobby.SetupReceiveNewRoomOwner(async newOwnerUserId =>
        {
            await OnMainThreadAsync(() =>
            {
                _room.OwnerUserId = newOwnerUserId;
                RoomOwnerId = newOwnerUserId;
                OnPropertyChanged(nameof(UserIsRoomOwner));
            });
            
            await toast.ShowMessageAsync("roomowner-changed");
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
                await toast.ShowSuccessAsync("player-updated");
                break;
            case PlayerType.Seeker:
                if (seekerIds.Contains(player.Id))
                    return;
                seekerIds.Add(player.Id);
                await toast.ShowSuccessAsync("player-updated");
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
        await OnMainThreadAsync(async () => await Clipboard.Default.SetTextAsync(text), force: true);
    }
}