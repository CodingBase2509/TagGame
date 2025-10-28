namespace TagGame.Shared.DTOs.Auth;

public sealed class TokenPairDto
{
    public string AccessToken { get; init; } = string.Empty;
    public DateTimeOffset AccessExpiresAt { get; init; }

    public string RefreshToken { get; init; } = string.Empty;
    public DateTimeOffset RefreshExpiresAt { get; init; }
}
