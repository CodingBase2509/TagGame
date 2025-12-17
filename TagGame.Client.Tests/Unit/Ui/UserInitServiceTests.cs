using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Ui.Services;

namespace TagGame.Client.Tests.Unit.Ui;

public class UserInitServiceTests
{
    private static (UserInitService sut, Mock<IAppPreferences> prefs, Mock<IAuthService> auth, Mock<IToastPublisher> toasts)
        Create()
    {
        var prefs = new Mock<IAppPreferences>();
        var auth = new Mock<IAuthService>();

        // Minimal host for ViewModelServiceBase toast handling
        var services = new ServiceCollection();
        var toasts = new Mock<IToastPublisher>();
        services.AddSingleton<IToastPublisher>(toasts.Object);
        SpUtils.Set(services.BuildServiceProvider());

        var sut = new UserInitService(prefs.Object, auth.Object);
        return (sut, prefs, auth, toasts);
    }

    [Fact]
    public void CreateDeviceId_formats_guid_and_device_name_without_spaces()
    {
        var (sut, prefs, _, _) = Create();
        prefs.SetupGet(p => p.DeviceName).Returns("  Pixel 8 Pro  ");

        var id = sut.CreateDeviceId();

        id.Should().Contain("_");
        var parts = id.Split('_', 2);
        parts.Length.Should().Be(2);
        Guid.TryParse(parts[0], out _).Should().BeTrue();
        parts[1].Should().Be("Pixel8Pro");
    }

    [Fact]
    public void ValidateInputs_returns_error_when_any_parameter_empty()
    {
        var (sut, _, _, _) = Create();

        var ok = sut.ValidateInputs("", null, out var errors);

        ok.Should().BeFalse();
        errors.Should().NotBeNull();
        errors!.Should().ContainSingle().Which.Should().Be("Errors.Validation.ParametersEmpty");
    }

    [Fact]
    public void ValidateInputs_collects_validation_errors_for_display_and_color()
    {
        var (sut, _, _, _) = Create();

        var ok = sut.ValidateInputs(" a", "#GGGGGG", out var errors);

        ok.Should().BeFalse();
        errors.Should().NotBeNull();
        errors!.Should().HaveCount(2);
        errors[0].Should().Be("Errors.Validation.DisplayName.NoEdgeSpaces");
        errors[1].Should().Be("Errors.Validation.AvatarColor.InvalidFormat");
    }

    [Fact]
    public void ValidateInputs_returns_true_when_all_valid()
    {
        var (sut, _, _, _) = Create();

        var ok = sut.ValidateInputs("Alex", "#FFAABB", out var errors);

        ok.Should().BeTrue();
        errors.Should().NotBeNull();
        errors!.Should().BeEmpty();
    }

    [Fact]
    public async Task InitializeUserAsync_returns_false_when_inputs_invalid()
    {
        var (sut, prefs, auth, _) = Create();

        var ok = await sut.InitializeUserAsync("dev id", "", "", CancellationToken.None);

        ok.Should().BeFalse();
        auth.VerifyNoOtherCalls();
        prefs.Verify(p => p.SetDeviceId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task InitializeUserAsync_generates_new_device_id_when_invalid_and_persists_on_success()
    {
        var (sut, prefs, auth, _) = Create();

        string? capturedDeviceId = null;
        auth.Setup(a => a.InitialAsync(It.IsAny<string>(), "Alex", "#FFAA00", It.IsAny<CancellationToken>()))
            .Callback<string, string, string, CancellationToken>((did, _, _, _) => capturedDeviceId = did)
            .ReturnsAsync(true);
        prefs.Setup(p => p.SetDeviceId(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ok = await sut.InitializeUserAsync(" invalid id ", "Alex", "#FFAA00", CancellationToken.None);

        ok.Should().BeTrue();
        capturedDeviceId.Should().NotBeNull();
        capturedDeviceId!.Should().NotBe(" invalid id ");
        Guid.TryParse(capturedDeviceId, out _).Should().BeTrue();
        prefs.Verify(p => p.SetDeviceId(capturedDeviceId!, It.IsAny<CancellationToken>()), Times.Once);
        auth.VerifyAll();
        prefs.VerifyAll();
    }

    [Fact]
    public async Task InitializeUserAsync_handles_network_error_and_returns_false()
    {
        var (sut, prefs, auth, toasts) = Create();
        auth.Setup(a => a.InitialAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("boom"));

        var ok = await sut.InitializeUserAsync("devId", "Alex", "#FFAABB", CancellationToken.None);

        ok.Should().BeFalse();
        prefs.Verify(p => p.SetDeviceId(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
