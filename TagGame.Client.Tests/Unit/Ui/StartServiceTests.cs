using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Features.Rooms;
using TagGame.Client.Core.Notifications;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Ui.Services;
using TagGame.Shared.DTOs.Rooms;

namespace TagGame.Client.Tests.Unit.Ui;

public class StartServiceTests
{
    private static (StartService Sut, Mock<IRoomsApi> Api, Mock<IToastPublisher> Toast, Mock<IQrCodeService> Qr) Create()
    {
        var api = new Mock<IRoomsApi>();
        var toast = new Mock<IToastPublisher>();
        var qr = new Mock<IQrCodeService>();

        var services = new ServiceCollection();
        services.AddSingleton<IToastPublisher>(toast.Object);
        SpUtils.Set(services.BuildServiceProvider());

        var sut = new StartService(api.Object, qr.Object);
        return (sut, api, toast, qr);
    }

    [Fact]
    public async Task CreateRoomAsync_shows_validation_error_when_name_invalid()
    {
        var (sut, api, toast, _) = Create();

        var result = await sut.CreateRoomAsync(" room");

        result.Should().BeNull();
        api.Verify(a => a.CreateRoomAsync(It.IsAny<CreateRoomRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        toast.Verify(t => t.PublishAsync(
                It.Is<ToastRequest>(r => r.Type == ToastType.Error && r.Message == "Errors.Validation.Rooms.Name.NoEdgeSpaces"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinRoomAsync_shows_validation_error_when_code_invalid()
    {
        var (sut, api, toast, _) = Create();

        var result = await sut.JoinRoomAsync("123");

        result.Should().BeNull();
        api.Verify(a => a.JoinRoomAsync(It.IsAny<JoinRoomRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        toast.Verify(t => t.PublishAsync(
                It.Is<ToastRequest>(r => r.Type == ToastType.Error && r.Message == "Errors.Validation.Rooms.AccessCode.MinLength"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateRoomAsync_calls_api_on_valid_input()
    {
        var (sut, api, _, _) = Create();
        api.Setup(a => a.CreateRoomAsync(It.IsAny<CreateRoomRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateRoomResponseDto { RoomId = Guid.NewGuid(), MembershipId = Guid.NewGuid(), Name = "Room" });

        var result = await sut.CreateRoomAsync("Room");

        result.Should().NotBeNull();
        api.Verify(a => a.CreateRoomAsync(
                It.Is<CreateRoomRequestDto>(r => r.Name == "Room"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task JoinRoomAsync_calls_api_on_valid_input()
    {
        var (sut, api, _, _) = Create();
        api.Setup(a => a.JoinRoomAsync(It.IsAny<JoinRoomRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new JoinRoomResponseDto { RoomId = Guid.NewGuid(), MembershipId = Guid.NewGuid(), Name = "Room" });

        var result = await sut.JoinRoomAsync("ABCDEFGH");

        result.Should().NotBeNull();
        api.Verify(a => a.JoinRoomAsync(
                It.Is<JoinRoomRequestDto>(r => r.AccessCode == "ABCDEFGH"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void IsAccessCodeFormatValid_respects_min_length()
    {
        var (sut, _, _, _) = Create();

        sut.IsAccessCodeFormatValid("1234567").Should().BeFalse();
        sut.IsAccessCodeFormatValid("12345678").Should().BeTrue();
    }

    [Fact]
    public async Task ScanQrCodeAsync_returns_value_from_service()
    {
        var (sut, _, _, qr) = Create();
        qr.Setup(q => q.ScanAsync(It.IsAny<CancellationToken>())).ReturnsAsync("CODE1234");

        var result = await sut.ScanQrCodeAsync();

        result.Should().Be("CODE1234");
    }
}
