using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Graphics;
using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Ui.ViewModels.Start;
using TagGame.Client.Core.Ui.Services;

namespace TagGame.Client.Tests.Unit.Ui;

public class UserInitViewModelTests
{
    private sealed class TestUiDispatcher : IUiDispatcher
    {
        public Task OnMainThreadAsync(Func<Task> action) => action();
        public Task OnMainThreadAsync(Action action)
        {
            action();
            return Task.CompletedTask;
        }
    }

    private static (UserInitViewModel vm, UserInitService service, Mock<IAuthService> auth, Mock<IAppPreferences> prefs, Mock<INavigationService> nav, Mock<IToastPublisher> toasts)
        Create()
    {
        var prefs = new Mock<IAppPreferences>();
        prefs.SetupGet(p => p.DeviceName).Returns("Test Device");
        var auth = new Mock<IAuthService>();
        var nav = new Mock<INavigationService>();
        var toasts = new Mock<IToastPublisher>();

        var services = new ServiceCollection();
        services.AddSingleton<IUiDispatcher>(new TestUiDispatcher());
        services.AddSingleton<INavigationService>(nav.Object);
        services.AddSingleton<IToastPublisher>(toasts.Object);
        SpUtils.Set(services.BuildServiceProvider());

        var service = new UserInitService(prefs.Object, auth.Object);
        var vm = new UserInitViewModel(service);
        return (vm, service, auth, prefs, nav, toasts);
    }

    [Fact]
    public void IsValid_reflects_display_and_avatar_color_inputs()
    {
        var (vm, _, _, _, _, _) = Create();

        vm.IsValid.Should().BeFalse(); // both empty

        vm.DisplayName = "Alex";
        vm.IsValid.Should().BeFalse(); // still missing color

        vm.AvatarColor = Colors.Orange;
        vm.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task SelectAvatarColor_sets_color_and_no_toast_for_valid()
    {
        var (vm, _, _, _, _, toasts) = Create();

        await vm.SelectAvatarColor(Colors.Green);

        vm.AvatarColor.Should().Be(Colors.Green);
        toasts.Verify(t => t.PublishAsync(It.IsAny<ToastRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeUserAsync_calls_auth_and_closes_modal_on_success()
    {
        var (vm, _, auth, prefs, nav, _) = Create();

        // Arrange: set valid inputs
        vm.DisplayName = "Alex";
        vm.AvatarColor = Colors.Orange;

        // Capture deviceId and arguments routed through UserInitService
        string? capturedDeviceId = null;
        string? capturedName = null;
        string? capturedColor = null;

        auth.Setup(a => a.InitialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((did, name, color, _) =>
            {
                capturedDeviceId = did; capturedName = name; capturedColor = color;
            })
            .ReturnsAsync(true);
        prefs.Setup(p => p.SetDeviceId(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        nav.Setup(n => n.CloseModalAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await vm.InitializeUserAsync();

        // Assert: args flowed to auth service; deviceId format Guid_Device
        capturedDeviceId.Should().NotBeNull();
        capturedDeviceId!.Should().Contain("_");
        var parts = capturedDeviceId!.Split('_', 2);
        parts.Length.Should().Be(2);
        Guid.TryParse(parts[0], out _).Should().BeTrue();
        parts[1].Should().Be("TestDevice");
        capturedName.Should().Be("Alex");
        capturedColor.Should().NotBeNull();
        capturedColor!.StartsWith('#').Should().BeTrue();

        // Modal closed on success
        nav.Verify(n => n.CloseModalAsync(It.IsAny<CancellationToken>()), Times.Once);
        auth.VerifyAll();
    }

    [Fact]
    public async Task InitializeUserAsync_does_not_close_modal_on_failure()
    {
        var (vm, _, auth, prefs, nav, _) = Create();

        vm.DisplayName = "Alex";
        vm.AvatarColor = Colors.Blue;

        auth.Setup(a => a.InitialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await vm.InitializeUserAsync();

        nav.Verify(n => n.CloseModalAsync(It.IsAny<CancellationToken>()), Times.Never);
        prefs.Verify(p => p.SetDeviceId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeUserAsync_is_not_reentrant_when_busy()
    {
        var (vm, _, auth, _, _, _) = Create();

        vm.DisplayName = "Sam";
        vm.AvatarColor = Colors.Red;

        // Hold the first call open
        var tcs = new TaskCompletionSource<bool>();
        auth.Setup(a => a.InitialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(() => tcs.Task);

        var firstCall = vm.InitializeUserAsync();

        // While busy, second call should no-op
        await Task.Delay(10);
        var secondCall = vm.InitializeUserAsync();
        secondCall.IsCompleted.Should().BeTrue();

        // Complete the first call
        tcs.SetResult(false);
        await firstCall;

        // Only one InitialAsync invocation
        auth.Verify(a => a.InitialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
