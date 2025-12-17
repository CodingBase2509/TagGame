using TagGame.Client.Infrastructure.Localization;
using TagGame.Client.Infrastructure.Themes;

namespace TagGame.Client.Infrastructure;

public class Init(
    ThemeInitializer themeInitializer,
    LocalizationInitializer localizationInitializer)
    : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private int _isInitialized;

    public void InitServices()
    {
        if (Interlocked.Exchange(ref _isInitialized, 1) == 1)
            return;

        // Keep this sync because it runs during CreateWindow. Ensure we don't deadlock by
        // avoiding capturing the UI context inside the initializers.
        themeInitializer.InitializeAsync(_cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
        localizationInitializer.InitializeAsync(_cts.Token).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }
}
