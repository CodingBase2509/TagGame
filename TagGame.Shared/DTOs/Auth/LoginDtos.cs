namespace TagGame.Shared.DTOs.Auth;

public sealed class LoginRequestDto
{
    public string DeviceId { get; set; } = string.Empty;
}

public sealed class LoginResponseDto
{
    public Guid UserId { get; set; }

    public TokenPairDto Tokens { get; set; } = new();
}
