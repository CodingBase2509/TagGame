using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using TagGame.Client.Clients;
using TagGame.Client.Ui.Navigation;
using TagGame.Shared.Domain.Games;
using INavigation = TagGame.Client.Ui.Navigation.INavigation;

namespace TagGame.Client.Ui.ViewModels;

[QueryProperty(nameof(Settings), "settings")]
[QueryProperty(nameof(CanEdit), "canEdit")]
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
            SetProperty(ref _hideTimeout, value.HideTimeout, nameof(HideTimeout));
            SetProperty(ref _isSeekerPingEnabled, value.IsPingEnabled, nameof(IsSeekerPingEnabled));
            if (value.IsPingEnabled)
                SetProperty(ref _seekerPingInterval, value.PingInterval.Value, nameof(SeekerPingInterval));
            SetProperty(ref _gameArea, value.Area, nameof(GameArea));
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditSeekerPing))]
    [NotifyPropertyChangedFor(nameof(CanNotEdit))]
    private bool _canEdit;
    public bool CanNotEdit => !CanEdit; 
    
    [ObservableProperty] 
    private TimeSpan _hideTimeout;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanEditSeekerPing))]
    private bool _isSeekerPingEnabled;

    public bool CanEditSeekerPing => CanEdit && IsSeekerPingEnabled;
    
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
    private void UpdateGameArea(IList<Location> points)
    {
        var locations = points
            .Select(point => new TagGame.Shared.Domain.Common.Location(point.Latitude, point.Longitude));
        
        GameArea.Boundary = locations.ToArray();
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