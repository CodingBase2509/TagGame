using TagGame.Shared.Validation;

namespace TagGame.Client.Core.Validation;

public static class UserInitFormValidator
{
    public static UserInitValidationResult Validate(string? deviceId, string? displayName, string? avatarColor)
    {
        UserInitRules.TryValidateDeviceId(deviceId, out var deviceError);
        UserInitRules.TryValidateDisplayName(displayName, out var displayNameError);
        UserInitRules.TryValidateAvatarColor(avatarColor, out var avatarError);

        var isValid = deviceError is null && displayNameError is null && avatarError is null;
        return new UserInitValidationResult(isValid, deviceError, displayNameError, avatarError);
    }
}
