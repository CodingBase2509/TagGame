using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TagGame.Client.Core.Http;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services.Abstractions;
using TagGame.Client.Core.Services.Implementations;
using TagGame.Client.Core.Storage;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Client.Tests.Unit.Auth;

public class AuthServiceTests
{
    private static (AuthService sut, Mock<IApiClient> api, Mock<ITokenStorage> store, Mock<IAppPreferences> prefs, AuthServiceOptions opts)
        Create()
    {
        var api = new Mock<IApiClient>(MockBehavior.Strict);
        var store = new Mock<ITokenStorage>(MockBehavior.Strict);
        var prefs = new Mock<IAppPreferences>();
        var opts = new AuthServiceOptions
        {
            AccessTokenRefreshSkew = TimeSpan.FromSeconds(30),
            RefreshRequestTimeout = TimeSpan.FromSeconds(5),
            RefreshPath = "/v1/auth/refresh",
            LogoutPath = "/v1/auth/logout"
        };
        var sut = new AuthService(api.Object, store.Object, prefs.Object, Options.Create(opts), TimeProvider.System, Mock.Of<ILogger<AuthService>>());
        return (sut, api, store, prefs, opts);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_returns_cached_when_not_expired()
    {
        var (sut, api, store, _, _) = Create();
        var tokens = new TokenPairDto
        {
            AccessToken = "acc",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshToken = "ref",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        store.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tokens);

        var token = await sut.GetValidAccessTokenAsync();

        token.Should().Be("acc");
        api.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_triggers_refresh_when_expired()
    {
        var (sut, api, store, _, opts) = Create();
        var expired = new TokenPairDto
        {
            AccessToken = "acc_old",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-10),
            RefreshToken = "ref",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        var refreshed = new TokenPairDto
        {
            AccessToken = "acc_new",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
            RefreshToken = "ref2",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(2)
        };
        // GetAsync is called before refresh, inside refresh, and after refresh
        var seq = new Queue<TokenPairDto?>(new[] { expired, expired, refreshed });
        store.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() => seq.Dequeue());
        api.Setup(a => a.PostAsync<RefreshRequestDto, RefreshResponseDto>(opts.RefreshPath,
            It.IsAny<RefreshRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RefreshResponseDto { Tokens = refreshed });
        store.Setup(s => s.SetAsync(refreshed, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var token = await sut.GetValidAccessTokenAsync();

        token.Should().Be("acc_new");
        api.VerifyAll();
        store.VerifyAll();
    }

    [Fact]
    public async Task RefreshAsync_clears_on_expired_refresh()
    {
        var (sut, api, store, _, _) = Create();
        var tokens = new TokenPairDto
        {
            AccessToken = "acc",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshToken = "ref",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1)
        };
        store.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tokens);
        store.Setup(s => s.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ok = await sut.RefreshAsync();
        ok.Should().BeFalse();
        store.VerifyAll();
        api.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LogoutAsync_posts_and_clears_tokens()
    {
        var (sut, api, store, prefs, opts) = Create();
        var tokens = new TokenPairDto
        {
            AccessToken = "acc",
            AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            RefreshToken = "ref",
            RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };
        store.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tokens);
        api.Setup(a => a.PostAsync<LogoutRequestDto, LogoutResponseDto>(opts.LogoutPath,
            It.Is<LogoutRequestDto>(r => r.RefreshToken == "ref"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LogoutResponseDto { Revoked = true });
        store.Setup(s => s.ClearAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await sut.LogoutAsync();

        api.VerifyAll();
        store.VerifyAll();
    }

    [Fact]
    public async Task LoginAsync_stores_tokens_and_returns_true()
    {
        var (sut, api, store, prefs, _) = Create();
        var resp = new LoginResponseDto
        {
            UserId = Guid.NewGuid(),
            Tokens = new TokenPairDto
            {
                AccessToken = "acc",
                AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                RefreshToken = "ref",
                RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            }
        };
        api.Setup(a => a.PostAsync<LoginRequestDto, LoginResponseDto>("/v1/auth/login",
            It.Is<LoginRequestDto>(r => r.DeviceId == "dev"), It.IsAny<CancellationToken>())).ReturnsAsync(resp);
        store.Setup(s => s.SetAsync(resp.Tokens, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        prefs.Setup(p => p.SetUserId(resp.UserId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ok = await sut.LoginAsync("dev");
        ok.Should().BeTrue();
        api.VerifyAll();
        store.VerifyAll();
    }

    [Fact]
    public async Task InitialAsync_stores_tokens_and_returns_true()
    {
        var (sut, api, store, _, _) = Create();
        var resp = new InitialResponseDto
        {
            UserId = Guid.NewGuid(),
            Tokens = new TokenPairDto
            {
                AccessToken = "acc",
                AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                RefreshToken = "ref",
                RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            }
        };
        api.Setup(a => a.PostAsync<InitialRequestDto, InitialResponseDto>("/v1/auth/initial",
            It.Is<InitialRequestDto>(r => r.DeviceId == "dev"), It.IsAny<CancellationToken>())).ReturnsAsync(resp);
        store.Setup(s => s.SetAsync(resp.Tokens, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var ok = await sut.InitialAsync("dev");
        ok.Should().BeTrue();
        api.VerifyAll();
        store.VerifyAll();
    }
}
