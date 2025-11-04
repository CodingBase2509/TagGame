namespace TagGame.Shared.DTOs.Users;

public class UserProfileDto
{
    public Guid Id { get; set; }

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    public string DeviceId { get; set; } = string.Empty;

    public string AvatarColor { get; set; } = string.Empty;
}
