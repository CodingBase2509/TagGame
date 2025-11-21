using TagGame.Client.Core.Services;
using TagGame.Shared.Validation;

namespace TagGame.Client.Core.Ui.ViewModels.Start;

public class UserInitService(IAppPreferences preferences, IAuthService authService)
    : ViewModelServiceBase
{
    public string CreateDeviceId() => $"{Guid.NewGuid()}_{preferences.DeviceName.Trim().Replace(" ", "")}";

    public bool ValidateInputs(string username, string? avatarColor, out string[]? errors)
    {
        var valid = true;
        errors = [];
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(avatarColor ?? string.Empty))
        {
            errors = ["Errors.Validation.ParametersEmpty"];
            return false;
        }

        if (!UserInitRules.TryValidateDisplayName(username, out var displayNameError))
        {
            valid = false;
            errors = [displayNameError!];
        }

        if (!UserInitRules.TryValidateAvatarColor(avatarColor, out var avatarColorError))
        {
            valid = false;
            errors = [.. errors, avatarColorError!];
        }

        return valid;
    }

    public bool ValidateAvatarColor(string? avatarColor, out string? error) => UserInitRules.TryValidateAvatarColor(avatarColor, out error);

    public async Task<bool> InitializeUserAsync(string deviceId, string username, string avatarColor, CancellationToken ct = default)
    {
        if (!ValidateInputs(username, avatarColor, out _))
            return false;

        if (!UserInitRules.TryValidateDeviceId(deviceId, out _))
            deviceId = Guid.NewGuid().ToString();

        var success = await RequestSafeHandle(() => authService.InitialAsync(deviceId, username, avatarColor, ct));
        if (success)
            await preferences.SetDeviceId(deviceId, ct);

        return success;
    }
}
