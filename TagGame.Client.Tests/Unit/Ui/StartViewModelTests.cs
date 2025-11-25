using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Ui.Services;
using TagGame.Client.Core.Ui.ViewModels.Start;

namespace TagGame.Client.Tests.Unit.Ui;

public class StartViewModelTests
{
    private sealed class TestUiDispatcher : IUiDispatcher
    {
        public Task OnMainThreadAsync(Func<Task> action) => action();
        public Task OnMainThreadAsync(Action action) { action(); return Task.CompletedTask; }
    }

    private static (StartViewModel vm, Mock<INavigationService> nav)
        Create(AppPreferencesSnapshot snap)
    {
        var prefs = new Mock<IAppPreferences>();
        prefs.SetupGet(p => p.Snapshot).Returns(snap);

        var nav = new Mock<INavigationService>();
        var service = new Mock<StartService>();

        var services = new ServiceCollection();
        services.AddSingleton<IUiDispatcher, TestUiDispatcher>();
        services.AddSingleton<INavigationService>(nav.Object);
        SpUtils.Set(services.BuildServiceProvider());

        var vm = new StartViewModel(prefs.Object, service.Object);
        return (vm, nav);
    }

    [Fact]
    public async Task InitializeAsync_opens_UserInit_modal_when_no_device()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "", Guid.Empty);
        var (vm, nav) = Create(snap);

        await vm.InitializeAsync();

        nav.Verify(n => n.OpenModalAsync(Routes.UserInit, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_skips_when_device_exists()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, nav) = Create(snap);

        await vm.InitializeAsync();

        nav.Verify(n => n.OpenModalAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

