using TagGame.Client.Core.Features.Rooms;
using TagGame.Client.Core.Services;
using TagGame.Shared.DTOs.Rooms;
using TagGame.Shared.Validation;

namespace TagGame.Client.Core.Ui.Services;

public class StartService(IRoomsApi api, IQrCodeService qrCode) : ViewModelServiceBase
{
    public async Task<CreateRoomResponseDto?> CreateRoomAsync(string? name, CancellationToken ct = default)
    {
        if (!RoomRules.TryValidateName(name, out var error))
        {
            await ShowValidationErrorAsync(error!);
            return null;
        }

        var request = new CreateRoomRequestDto { Name = name!.Trim() };
        return await RequestSafeHandle(() => api.CreateRoomAsync(request, ct));
    }

    public async Task<JoinRoomResponseDto?> JoinRoomAsync(string? accessCode, CancellationToken ct = default)
    {
        if (!RoomRules.TryValidateAccessCode(accessCode, out var error))
        {
            await ShowValidationErrorAsync(error!);
            return null;
        }

        var request = new JoinRoomRequestDto { AccessCode = accessCode!.Trim() };
        return await RequestSafeHandle(() => api.JoinRoomAsync(request, ct));
    }

    public bool IsAccessCodeFormatValid(string? accessCode) =>
        RoomRules.TryValidateAccessCode(accessCode, out _);

    public bool IsRoomNameFormatValid(string name) =>
        RoomRules.TryValidateName(name, out _);

    public async Task<string?> ScanQrCodeAsync(CancellationToken ct = default)
    {
        var accessCode = await qrCode.ScanAsync(ct);
        return accessCode;
    }

    private static Task ShowValidationErrorAsync(string error)
    {
        var toast = SpUtils.GetRequiredService<IToastPublisher>();
        return toast.Error(error);
    }
}
