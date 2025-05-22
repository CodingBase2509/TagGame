using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Graphics;
using TagGame.Client.Clients;
using TagGame.Client.Services;
using TagGame.Client.Ui.ToastMessages;

namespace TagGame.Client.Ui.ViewModels;

public partial class InitPageViewModel(
    ConfigHandler config, 
    RestClient api, 
    INavigation nav,
    IToastService toast) : ViewModelBase
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

    public override async Task InitializeAsync()
    {
        await config.InitAsync();
        
        var userConfig = await config.ReadAsync<UserConfig>();
        var serverConfig = await config.ReadAsync<ServerConfig>();
        if (serverConfig is not null)
        {
            ServerAddress = serverConfig.Host;
            if (serverConfig.Port is not null && serverConfig.Port.Value > 0)
                ServerAddress += $":{serverConfig.Port}";
        }
        
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
            await toast.ShowErrorAsync("failed-create-user");
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
        host = splitAddress.Length switch
        {
            1 when !splitAddress[0].StartsWith("http") => splitAddress[0].Replace("/", ""),
            > 1 when splitAddress[0].StartsWith("http") => string.Join(':', splitAddress[0], splitAddress[1]),
            > 1 when !splitAddress[0].StartsWith("http") => splitAddress[0].Replace("/", ""),
            _ => string.Empty
        };

        if (splitAddress.Length > 1 && int.TryParse(splitAddress[^1], out var portValue))
            port = portValue;
        else
            port = ServerConfig.Default.Port;
        
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