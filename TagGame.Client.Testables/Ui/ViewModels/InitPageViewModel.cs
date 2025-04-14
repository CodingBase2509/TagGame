using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.ViewModels;

public partial class InitPageViewModel(ConfigHandler config, RestClient api, INavigation nav) : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmEnabled))]
    private string username = string.Empty;
    
    [ObservableProperty]
    private string serverAddress = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmEnabled))]
    private Color avatarColor = Colors.Transparent;
    
    public bool IsConfirmEnabled => !string.IsNullOrEmpty(username) && 
                                    !Equals(avatarColor, Colors.Transparent);

    public async Task IsInitializedAsync()
    {
        var userConfig = await config.ReadAsync<UserConfig>();
        var serverConfig = await config.ReadAsync<ServerConfig>();
        if (userConfig is not null && serverConfig is not null && config.CanInteractWithFiles)
            await nav.GoToStart(NavigationMode.Parallel);
    }

    [RelayCommand]
    private void SetAvatarColor(Color color) =>
        AvatarColor = Equals(color, AvatarColor) ? Colors.Transparent : color;

    [RelayCommand]
    private async Task InitAsync()
    {
        await CreateAndWriteServerConfigAsync();
        var success = await SetupUserAsync();
        if (!success)
        {
            // show error
            return;
        }
        
        await nav.GoToStart(NavigationMode.Parallel);
    }

    private async Task CreateAndWriteServerConfigAsync()
    {
        string host = string.Empty;
        int? port = null;
        
        if (string.IsNullOrEmpty(serverAddress))
            host = ServerConfig.Default.Host;
        
        var splitAddress = serverAddress.Split(':');
        host = splitAddress[0];

        if (splitAddress.Length > 1 && int.TryParse(splitAddress[1], out var portValue))
            port = portValue;
        
        await config.WriteAsync(new ServerConfig()
        {
            Host = host,
            Port = port,
        });
    }

    private async Task<bool> SetupUserAsync()
    {
        var response = await api.CreateUserAsync(new()
        {
            Name = username,
            AvatarColor = avatarColor.ToColorDTO(),
        });

        if (!response.IsSuccess)
            return false;

        await config.WriteAsync(new UserConfig()
        {
            UserId = response.Value.Id,
            Username = response.Value.DefaultName,
            AvatarColor = response.Value.DefaultAvatarColor.ToMauiColor(),
        });

        return true;
    }
}