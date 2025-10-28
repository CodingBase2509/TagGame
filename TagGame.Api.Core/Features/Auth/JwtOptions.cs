namespace TagGame.Api.Core.Features.Auth;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;

    public int AccessMinutes { get; set; } = 20;
    public int RefreshDays { get; set; } = 30;
}

