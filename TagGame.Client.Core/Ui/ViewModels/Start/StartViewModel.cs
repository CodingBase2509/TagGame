using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Core.Ui.ViewModels.Start;

public class StartViewModel(IAppPreferences preferences) : ViewModelBase
{
    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var deviceId = preferences.Snapshot.DeviceId;
        if (!string.IsNullOrEmpty(deviceId))
            return;

        await Navigation.OpenModalAsync(Routes.UserInit, null, cancellationToken);
    }
}
