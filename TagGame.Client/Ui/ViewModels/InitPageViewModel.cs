using CommunityToolkit.Mvvm.ComponentModel;
using TagGame.Client.Services;

namespace TagGame.Client.Ui.ViewModels;

public partial class InitPageViewModel : ViewModelBase
{
    private readonly ConfigHandler config;
    private readonly RestClient api;

    [ObservableProperty]
    private string username;
    
    [ObservableProperty]
    private string serverAddress;
    
    [ObservableProperty]
    private Color avatarColor;
    
    public event EventHandler InitCompleted;
    
    public InitPageViewModel(ConfigHandler config, RestClient api)
    {
        this.config = config;
        this.api = api;
    }
    
    public async Task IsInitializedAsync()
    {
        var userConfig = await config.ReadAsync<UserConfig>();
        if (userConfig is not null)
            InitCompleted.Invoke(this, EventArgs.Empty);
    }
    
    public async Task InitAsync()
    {
        
    }
}