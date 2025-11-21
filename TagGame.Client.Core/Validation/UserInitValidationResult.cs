namespace TagGame.Client.Core.Validation;

public sealed record UserInitValidationResult(
    bool IsValid,
    string? DeviceIdError,
    string? DisplayNameError,
    string? AvatarColorError);
