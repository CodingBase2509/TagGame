namespace TagGame.Client.Core.Options;

public class AuthServiceOptions
{
    public TimeSpan AccessTokenRefreshSkew { get; init; } = TimeSpan.FromMinutes(1);

    public TimeSpan RefreshRequestTimeout { get; init; } = TimeSpan.FromSeconds(10);

    public string RefreshPath { get; init; } = "/auth/refresh";

    public string LogoutPath { get; init; } = "/auth/logout";
}
