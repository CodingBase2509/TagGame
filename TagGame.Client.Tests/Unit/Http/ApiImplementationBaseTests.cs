using Microsoft.Extensions.DependencyInjection;
using TagGame.Client.Core.Http;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;
using TagGame.Client.Core.Storage;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Client.Tests.Unit.Http;

public class ApiImplementationBaseTests
{
    private sealed class TestApi : ApiImplementationBase
    {
        public Task InvokeAsync(CancellationToken ct = default) => EnsureValidToken(ct);
    }

    private static (TestApi Sut, Mock<ITokenStorage> Storage, Mock<IAuthService> Auth) Create(TokenPairDto? tokens)
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        var auth = new Mock<IAuthService>();
        auth.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        auth.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var prefs = new Mock<IAppPreferences>();
        prefs.SetupGet(p => p.Snapshot)
            .Returns(new AppPreferencesSnapshot(ThemeMode.System, Language.English, true, "device-1", Guid.NewGuid()));

        var services = new ServiceCollection();
        services.AddSingleton(storage.Object);
        services.AddSingleton(auth.Object);
        services.AddSingleton(prefs.Object);
        SpUtils.Set(services.BuildServiceProvider());

        return (new TestApi(), storage, auth);
    }

    [Fact]
    public async Task EnsureValidToken_skips_when_tokens_valid()
    {
        var tokens = new TokenPairDto
        {
            AccessToken = "a",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshToken = "r",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        var (sut, _, auth) = Create(tokens);

        await sut.InvokeAsync();

        auth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        auth.Verify(a => a.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureValidToken_refreshes_when_access_expired()
    {
        var tokens = new TokenPairDto
        {
            AccessToken = "a",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            RefreshToken = "r",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        var (sut, _, auth) = Create(tokens);

        await sut.InvokeAsync();

        auth.Verify(a => a.RefreshAsync(It.IsAny<CancellationToken>()), Times.Once);
        auth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureValidToken_logs_in_when_refresh_expired()
    {
        var tokens = new TokenPairDto
        {
            AccessToken = "a",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshToken = "r",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };

        var (sut, _, auth) = Create(tokens);

        await sut.InvokeAsync();

        auth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        auth.Verify(a => a.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnsureValidToken_logs_in_when_tokens_missing()
    {
        var (sut, _, auth) = Create(null);

        await sut.InvokeAsync();

        auth.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        auth.Verify(a => a.RefreshAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
