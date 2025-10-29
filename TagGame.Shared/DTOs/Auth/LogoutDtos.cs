namespace TagGame.Shared.DTOs.Auth;

public sealed class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class LogoutResponseDto
{
    public bool Revoked { get; set; }
}
