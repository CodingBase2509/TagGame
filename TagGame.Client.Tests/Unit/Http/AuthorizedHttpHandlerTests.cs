using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using TagGame.Client.Core.Http;
using TagGame.Client.Core.Options;
using TagGame.Client.Core.Storage;
using TagGame.Shared.DTOs.Auth;

namespace TagGame.Client.Tests.Unit.Http;

public class AuthorizedHttpHandlerTests
{
    [Fact]
    public async Task Attaches_bearer_when_token_available()
    {
        using var inner = new StubHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenPairDto
            {
                AccessToken = "tok",
                AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                RefreshToken = "refresh",
                RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            });

        using var sut = new AuthorizedHttpHandler(storage.Object, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };

        using var invoker = new HttpMessageInvoker(sut);
        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/api"), default);

        inner.LastRequest!.Headers.Authorization.Should().Be(new AuthenticationHeaderValue("Bearer", "tok"));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Swallows_storage_errors_without_header()
    {
        var storage = new Mock<ITokenStorage>();
        storage.SetupSequence(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"))
            .ReturnsAsync(new TokenPairDto
            {
                AccessToken = "tok2",
                AccessExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
                RefreshToken = "refresh2",
                RefreshExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            });

        using var inner = new StubHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = new AuthorizedHttpHandler(storage.Object, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(sut);

        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/x"), default);
        inner.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task No_token_no_header()
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAsync(It.IsAny<CancellationToken>())).ReturnsAsync((TokenPairDto?)null);

        using var inner = new StubHandler(req => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = new AuthorizedHttpHandler(storage.Object, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(sut);

        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://localhost/protected"), default);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        inner.LastRequest!.Headers.Authorization.Should().BeNull();
    }

    [Fact]
    public async Task Does_not_throw_when_storage_throws()
    {
        var storage = new Mock<ITokenStorage>();
        storage.Setup(s => s.GetAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("storage down"));

        using var inner = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = new AuthorizedHttpHandler(storage.Object, Mock.Of<ILogger<AuthorizedHttpHandler>>())
        { InnerHandler = inner };
        using var invoker = new HttpMessageInvoker(sut);

        var resp = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Post, "http://localhost/v1/auth/login"), default);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
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

}
