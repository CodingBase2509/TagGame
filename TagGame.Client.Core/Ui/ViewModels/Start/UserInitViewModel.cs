namespace TagGame.Client.Core.Ui.ViewModels.Start;

public sealed partial class UserInitViewModel(UserInitService service) : ViewModelBase
{
    private readonly string _deviceId = service.CreateDeviceId();

    public bool IsValid => service.ValidateInputs(DisplayName, AvatarColor?.ToArgbHex(), out _);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private string _displayName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsValid))]
    private Color? _avatarColor;

    [RelayCommand]
    public async Task SelectAvatarColor(Color color)
    {
        var valid = service.ValidateAvatarColor(color.ToArgbHex(), out var error);
        if (!valid)
            await Toast.Error(error!);

        AvatarColor = color;
    }

    [RelayCommand(CanExecute = nameof(IsValid))]
    public Task InitializeUserAsync()
    {
        if (IsBusy)
            return Task.CompletedTask;

        return RunBusyAsync(async token =>
        {
            var success = await service.InitializeUserAsync(_deviceId, DisplayName, AvatarColor!.ToArgbHex(), token)
                .ConfigureAwait(false);

            if (!success)
                return;

            await Navigation.CloseModalAsync(token).ConfigureAwait(false);
        });
    }
}
