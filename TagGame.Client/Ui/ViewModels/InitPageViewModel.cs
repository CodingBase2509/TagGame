using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.ViewModels;

public partial class InitPageViewModel : ViewModelBase
{
    private readonly ConfigHandler config;
    private readonly RestClient api;

    [ObservableProperty]
    private string username = string.Empty;
    
    [ObservableProperty]
    private string serverAddress = string.Empty;
    
    [ObservableProperty]
    private Color avatarColor = Colors.Transparent;
    
    public InitPageViewModel(ConfigHandler config, RestClient api)
    {
        this.config = config;
        this.api = api;
    }
    
    public async Task IsInitializedAsync()
    {
        var userConfig = await config.ReadAsync<UserConfig>();
        var serverConfig = await config.ReadAsync<ServerConfig>();
        if (userConfig is not null && serverConfig is not null)
            await Shell.Current.GoToAsync("//start");
    }
    
    [RelayCommand]
    private void SetAvatarColor(Color color) =>
        AvatarColor = color;

    [RelayCommand]
    private async Task InitAsync()
    {
        
        await Shell.Current.GoToAsync("//start");
    }
}