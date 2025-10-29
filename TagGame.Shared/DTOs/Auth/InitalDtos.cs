namespace TagGame.Shared.DTOs.Auth;

public sealed class InitialRequestDto
{
    public string DeviceId { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? AvatarColor { get; set; }
}

public sealed class InitialResponseDto
{
    public Guid UserId { get; set; }
    public TokenPairDto Tokens { get; set; } = new();
}
