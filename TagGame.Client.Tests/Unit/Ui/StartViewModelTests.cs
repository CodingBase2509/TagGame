using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Features.Rooms;
using TagGame.Client.Core.Navigation;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Ui.Services;
using TagGame.Client.Core.Ui.ViewModels.Start;
using TagGame.Shared.DTOs.Rooms;

namespace TagGame.Client.Tests.Unit.Ui;

public class StartViewModelTests
{
    private sealed class TestUiDispatcher : IUiDispatcher
    {
        public Task OnMainThreadAsync(Func<Task> action) => action();
        public Task OnMainThreadAsync(Action action) { action(); return Task.CompletedTask; }
    }

    private static (StartViewModel vm, Mock<INavigationService> nav, Mock<IRoomsApi> api, Mock<IQrCodeService> qr, Mock<IToastPublisher> toast)
        Create(AppPreferencesSnapshot snap)
    {
        var prefs = new Mock<IAppPreferences>();
        prefs.SetupGet(p => p.Snapshot).Returns(snap);

        var nav = new Mock<INavigationService>();
        var api = new Mock<IRoomsApi>();
        var qr = new Mock<IQrCodeService>();
        var toast = new Mock<IToastPublisher>();

        var services = new ServiceCollection();
        services.AddSingleton<IUiDispatcher, TestUiDispatcher>();
        services.AddSingleton<INavigationService>(nav.Object);
        services.AddSingleton<IToastPublisher>(toast.Object);
        SpUtils.Set(services.BuildServiceProvider());

        var startService = new StartService(api.Object, qr.Object);

        var vm = new StartViewModel(prefs.Object, startService);
        return (vm, nav, api, qr, toast);
    }

    [Fact]
    public async Task InitializeAsync_opens_UserInit_modal_when_no_device()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "", Guid.Empty);
        var (vm, nav, _, _, _) = Create(snap);

        await vm.InitializeAsync();

        nav.Verify(n => n.OpenModalAsync(Routes.UserInit, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_skips_when_device_exists()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, nav, _, _, _) = Create(snap);

        await vm.InitializeAsync();

        nav.Verify(n => n.OpenModalAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void CanCreateRoom_follows_room_name_rules()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, _, _, _, _) = Create(snap);

        vm.RoomName = "A";
        vm.CanCreateRoom.Should().BeFalse();

        vm.RoomName = "Room";
        vm.CanCreateRoom.Should().BeTrue();
    }

    [Fact]
    public void CanJoinRoom_requires_eight_characters()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, _, _, _, _) = Create(snap);

        vm.AccessCode = "1234567";
        vm.CanJoinRoom.Should().BeFalse();
        vm.JoinRoomByCodeCommand.CanExecute(null).Should().BeFalse();

        vm.AccessCode = "12345678";
        vm.CanJoinRoom.Should().BeTrue();
        vm.JoinRoomByCodeCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task CreateRoomCommand_navigates_to_lobby_on_success()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, nav, api, _, _) = Create(snap);
        var response = new CreateRoomResponseDto
        {
            RoomId = Guid.NewGuid(),
            MembershipId = Guid.NewGuid(),
            Name = "Room"
        };
        api.Setup(a => a.CreateRoomAsync(It.IsAny<CreateRoomRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        vm.RoomName = "Room";
        await vm.CreateRoomCommand.ExecuteAsync(null);

        api.Verify(a => a.CreateRoomAsync(
                It.Is<CreateRoomRequestDto>(r => r.Name == "Room"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        nav.Verify(n => n.GoToAsync(
                Routes.Lobby,
                It.Is<IReadOnlyDictionary<string, object?>?>(d =>
                    d!["RoomId"]!.Equals(response.RoomId) &&
                    d["RoomName"]!.Equals(response.Name) &&
                    d["MembershipId"]!.Equals(response.MembershipId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinRoomByCodeCommand_navigates_to_lobby_on_success()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, nav, api, _, _) = Create(snap);
        var response = new JoinRoomResponseDto
        {
            RoomId = Guid.NewGuid(),
            MembershipId = Guid.NewGuid(),
            Name = "Joined"
        };
        api.Setup(a => a.JoinRoomAsync(It.IsAny<JoinRoomRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        vm.AccessCode = "ABCDEFGH";
        await vm.JoinRoomByCodeCommand.ExecuteAsync(null);

        api.Verify(a => a.JoinRoomAsync(
                It.Is<JoinRoomRequestDto>(r => r.AccessCode == "ABCDEFGH"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        nav.Verify(n => n.GoToAsync(
                Routes.Lobby,
                It.Is<IReadOnlyDictionary<string, object?>?>(d =>
                    d!["RoomId"]!.Equals(response.RoomId) &&
                    d["RoomName"]!.Equals(response.Name) &&
                    d["MembershipId"]!.Equals(response.MembershipId)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinRoomByQrCodeCommand_shows_toast_when_scan_fails()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, nav, _, qr, toast) = Create(snap);
        qr.Setup(q => q.ScanAsync(It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

        await vm.JoinRoomByQrCodeCommand.ExecuteAsync(null);

        toast.Verify(t => t.PublishAsync(
                It.Is<ToastRequest>(r => r.Type == ToastType.Error && r.Message == "Start.QrCode.Error"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        nav.Verify(n => n.GoToAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object?>?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task JoinRoomByQrCodeCommand_sets_code_and_joins_when_scan_succeeds()
    {
        var snap = new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "dev", Guid.NewGuid());
        var (vm, nav, api, qr, _) = Create(snap);
        qr.Setup(q => q.ScanAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ABCDEFGH");
        api.Setup(a => a.JoinRoomAsync(It.IsAny<JoinRoomRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JoinRoomResponseDto
            {
                RoomId = Guid.NewGuid(),
                MembershipId = Guid.NewGuid(),
                Name = "Room"
            });

        await vm.JoinRoomByQrCodeCommand.ExecuteAsync(null);

        vm.AccessCode.Should().Be("ABCDEFGH");
        api.Verify(a => a.JoinRoomAsync(
                It.Is<JoinRoomRequestDto>(r => r.AccessCode == "ABCDEFGH"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        nav.Verify(n => n.GoToAsync(
                Routes.Lobby,
                It.IsAny<IReadOnlyDictionary<string, object?>?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
