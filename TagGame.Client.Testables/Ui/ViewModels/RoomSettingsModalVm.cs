using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using TagGame.Client.Clients;
using TagGame.Client.Ui.Navigation;
using TagGame.Shared.Domain.Games;
using INavigation = TagGame.Client.Ui.Navigation.INavigation;

namespace TagGame.Client.Ui.ViewModels;

[QueryProperty(nameof(Settings), "settings")]
public partial class RoomSettingsModalVm(
    LobbyClient lobby,
    INavigation nav) : ViewModelBase
{
    private GameSettings _settings;
    public GameSettings Settings
    {
        get => _settings;
        set
        {
            SetProperty(ref _settings, value);
            SetProperty(ref _hideTimeout, value.HideTimeout);
            SetProperty(ref _isSeekerPingEnabled, value.IsPingEnabled);
            SetProperty(ref _seekerPingInterval, value.PingInterval ?? TimeSpan.Zero);
            SetProperty(ref _gameArea, value.Area);
        }
    }
    
    [ObservableProperty] 
    private TimeSpan _hideTimeout;

    [ObservableProperty] 
    private bool _isSeekerPingEnabled;

    [ObservableProperty] 
    private TimeSpan _seekerPingInterval;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameAreaSelected))]
    private GameArea _gameArea;

    public bool GameAreaSelected => _gameArea.Boundary.Length > 0;

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await nav.GoToLobby(NavigationMode.Backward);
    }
    
    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        _settings.HideTimeout = HideTimeout;
        _settings.IsPingEnabled = IsSeekerPingEnabled;
        _settings.PingInterval = SeekerPingInterval;
        _settings.Area = GameArea;

        await lobby.UpdateGameSettingsAsync(_settings);
        
        await GoBackAsync();
    }
}