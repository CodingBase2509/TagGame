namespace TagGame.Shared.DTOs.Auth;

public sealed class RefreshRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class RefreshResponseDto
{
    public TokenPairDto Tokens { get; set; } = new();
}
