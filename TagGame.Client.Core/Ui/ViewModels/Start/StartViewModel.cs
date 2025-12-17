using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Ui.Services;

namespace TagGame.Client.Core.Ui.ViewModels.Start;

public partial class StartViewModel(IAppPreferences preferences, StartService service) : ViewModelBase
{
    [NotifyPropertyChangedFor(nameof(CanCreateRoom))]
    [ObservableProperty]
    private string _roomName = string.Empty;

    [NotifyPropertyChangedFor(nameof(CanJoinRoom))]
    [ObservableProperty]
    private string _accessCode = string.Empty;

    public bool CanCreateRoom => service.IsRoomNameFormatValid(RoomName);

    public bool CanJoinRoom => service.IsAccessCodeFormatValid(AccessCode);

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var deviceId = preferences.Snapshot.DeviceId;
        if (!string.IsNullOrEmpty(deviceId))
            return;

        await Navigation.OpenModalAsync(Routes.UserInit, null, cancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanCreateRoom))]
    private async Task CreateRoomAsync()
    {
        var createResponse = await RunBusyAsync(ct => service.CreateRoomAsync(RoomName, ct));
        if (createResponse is null)
            return;

        await Navigation.GoToAsync(
            Routes.Lobby,
            new Dictionary<string, object?>
            {
                { "RoomId", createResponse.RoomId },
                { "RoomName", createResponse.Name },
                { "MembershipId", createResponse.MembershipId }
            },
            ViewModelCancellationToken);
    }

    [RelayCommand(CanExecute = nameof(CanJoinRoom))]
    private async Task JoinRoomByCodeAsync()
    {
        var joinResponse = await RunBusyAsync(ct => service.JoinRoomAsync(AccessCode, ct));
        if (joinResponse is null)
            return;

        await Navigation.GoToAsync(
            Routes.Lobby,
            new Dictionary<string, object?>
            {
                { "RoomId", joinResponse.RoomId },
                { "RoomName", joinResponse.Name },
                { "MembershipId", joinResponse.MembershipId }
            },
            ViewModelCancellationToken);
    }

    [RelayCommand]
    private async Task JoinRoomByQrCodeAsync()
    {
        var code = await service.ScanQrCodeAsync(ViewModelCancellationToken);
        if (code is null)
        {
            await Toast.Error("Start.QrCode.Error");
            return;
        }

        await RunOnUiThreadAsync(() => AccessCode = code);

        await JoinRoomByCodeAsync();
    }

    [RelayCommand]
    private async Task OpenSettingsAsync() =>
        await Navigation.GoToAsync(Routes.Settings, null, ViewModelCancellationToken);
}
