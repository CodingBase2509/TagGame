namespace TagGame.Shared.DTOs.Users;

public class PatchUserAccountDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string AvatarColor { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string? DeviceId { get; set; } = string.Empty;
}
