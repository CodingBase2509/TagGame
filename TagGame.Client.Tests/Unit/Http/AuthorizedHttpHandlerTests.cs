using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using TagGame.Client.Core.Http;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Services;

namespace TagGame.Client.Tests.Unit.Http;

public class AuthorizedHttpHandlerTests
{
    [Fact]
    public async Task Attaches_bearer_when_token_available()
    {
        var auth = new Mock<IAuthService>();
        auth.Setup(a => a.GetValidAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("tok");
        var prefs = new FakePrefs();
        using var inner = new StubHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = new AuthorizedHttpHandler(auth.Object, prefs, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };

        using var invoker = new HttpMessageInvoker(sut);
        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/api"), default);

        inner.LastRequest!.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Bearer", "tok"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Pre_refresh_then_attach_token_when_initially_missing()
    {
        var auth = new Mock<IAuthService>();
        var calls = 0;
        auth.Setup(a => a.GetValidAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
        {
            calls++;
            return calls == 1 ? null : "tok2";
        });
        auth.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var prefs = new FakePrefs();

        using var inner = new StubHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = new AuthorizedHttpHandler(auth.Object, prefs, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(sut);

        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/x"), default);
        inner.LastRequest!.Headers.Authorization!.Parameter.Should().Be("tok2");
    }

    [Fact]
    public async Task On_401_refresh_and_retry_idempotent()
    {
        var auth = new Mock<IAuthService>();
        auth.SetupSequence(a => a.GetValidAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("tok1").ReturnsAsync("tokNew");
        auth.Setup(a => a.RefreshAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var prefs = new FakePrefs();

        var first = true;
        using var inner = new StubHandler(req =>
        {
            if (first) { first = false; return new HttpResponseMessage(HttpStatusCode.Unauthorized); }
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var sut = new AuthorizedHttpHandler(auth.Object, prefs, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(sut);

        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/protected"), default);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Auth_paths_are_passed_through()
    {
        var auth = new Mock<IAuthService>(MockBehavior.Strict);
        var prefs = new FakePrefs();
        using var inner = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = new AuthorizedHttpHandler(auth.Object, prefs, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(sut);

        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/v1/auth/login"), default);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        auth.VerifyNoOtherCalls();
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(responder(request));
        }
    }

    private sealed class FakePrefs : IAppPreferences
    {
        public AppPreferencesSnapshot Snapshot { get; private set; } = new(ThemeMode.System, Language.English, false, Guid.NewGuid(), Guid.Empty);
        public event EventHandler<AppPreferencesSnapshot>? PreferencesChanged
        {
            add { }
            remove { }
        }
        public Task ChangeLanguageAsync(Language newLanguage, CancellationToken ct = default) => Task.CompletedTask;
        public Task ChangeThemeAsync(ThemeMode newTheme, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetDeviceId(Guid id, CancellationToken ct = default) { Snapshot = Snapshot with { DeviceId = id }; return Task.CompletedTask; }
        public Task SetNotificationsEnabledAsync(bool enabled, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetUserId(Guid id, CancellationToken ct = default) { Snapshot = Snapshot with { UserId = id }; return Task.CompletedTask; }
    }
}
