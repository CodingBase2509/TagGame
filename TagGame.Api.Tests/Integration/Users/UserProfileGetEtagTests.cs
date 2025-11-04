using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using TagGame.Api.Core.Common.Http;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.DTOs.Auth;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Tests.Integration.Users;

public sealed class UserProfileGetEtagTests : IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;

    public override async Task InitializeAsync()
    {
        UseDbTestContainer();
        await base.InitializeAsync();
        if (_dbContainer is not null)
        {
            var cs = await CreateDatabaseAsync("api_users_get");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
            Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", cs);
        }

        _factory = new WebApplicationFactory<Program>();

        using var scope = _factory.Services.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        if ((await auth.Database.GetPendingMigrationsAsync()).Any())
            await auth.Database.MigrateAsync();
    }

    public override async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        await base.DisposeAsync();
    }

    [Fact]
    public async Task Get_Own_Profile_Returns_200_With_Etag()
    {
        var (client, userId, _) = await CreateAuthedClientAsync();

        var resp = await client.GetAsync("/v1/users/me");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Headers.ETag.Should().NotBeNull();

        var dto = await resp.Content.ReadFromJsonAsync<UserProfileDto>();
        dto.Should().NotBeNull();
        dto!.Id.Should().Be(userId);

        // ETag should equal current DB token
        var cc = await GetCurrentConcurrencyTokenAsync(userId);
        EtagUtils.TryParseStringEtag(resp.Headers.ETag!.Tag, out var parsed).Should().BeTrue();
        parsed.Should().Be(cc);
    }

    [Fact]
    public async Task Get_With_IfNoneMatch_NotModified_304()
    {
        var (client, userId, _) = await CreateAuthedClientAsync();
        // First GET to obtain ETag
        var first = await client.GetAsync("/v1/users/me");
        first.EnsureSuccessStatusCode();
        var etag = first.Headers.ETag!.Tag;

        // Second GET with If-None-Match
        var req = new HttpRequestMessage(HttpMethod.Get, "/v1/users/me");
        req.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.NotModified);
        resp.Headers.ETag.Should().NotBeNull();
        // Still current
        var cc = await GetCurrentConcurrencyTokenAsync(userId);
        EtagUtils.TryParseStringEtag(resp.Headers.ETag!.Tag, out var parsed).Should().BeTrue();
        parsed.Should().Be(cc);
    }

    [Fact]
    public async Task Get_With_Invalid_IfNoneMatch_Returns_400()
    {
        var (client, _, _) = await CreateAuthedClientAsync();

        var req = new HttpRequestMessage(HttpMethod.Get, "/v1/users/me");
        req.Headers.TryAddWithoutValidation("If-None-Match", "not-base64");
        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<(HttpClient client, Guid userId, string accessToken)> CreateAuthedClientAsync()
    {
        var client = _factory!.CreateClient();
        var device = Guid.NewGuid().ToString("N");
        var init = await client.PostAsJsonAsync("/v1/auth/initial", new InitialRequestDto { DeviceId = device });
        init.EnsureSuccessStatusCode();
        var body = await init.Content.ReadFromJsonAsync<InitialResponseDto>();
        var token = body!.Tokens.AccessToken;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (client, body.UserId, token);
    }

    private async Task<uint> GetCurrentConcurrencyTokenAsync(Guid userId)
    {
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var user = await db.Users.FindAsync(userId);
        user.Should().NotBeNull();
        return db.Entry(user!).Property<uint>("xmin").CurrentValue;
    }
}

