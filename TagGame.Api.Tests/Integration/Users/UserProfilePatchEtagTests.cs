using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TagGame.Api.Core.Common.Http;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.DTOs.Auth;
using TagGame.Shared.DTOs.Users;

namespace TagGame.Api.Tests.Integration.Users;

public sealed class UserProfilePatchEtagTests : IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;

    public override async Task InitializeAsync()
    {
        UseDbTestContainer();
        await base.InitializeAsync();
        if (_dbContainer is not null)
        {
            var cs = await CreateDatabaseAsync("api_users_profile");
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
    public async Task Patch_With_IfMatch_Succeeds_And_Returns_New_Etag()
    {
        var (client, userId, _) = await CreateAuthedClientAsync();
        var ccToken = await GetCurrentConcurrencyTokenAsync(userId);
        var ifMatch = EtagUtils.ToEtag(ccToken);

        var dto = new PatchUserAccountDto { DisplayName = "NewName" };
        var resp = await PatchProfileAsync(client, ifMatch, dto);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
        resp.Headers.ETag.Should().NotBeNull();
        EtagUtils.TryParseStringEtag(resp.Headers.ETag!.Tag, out var newToken).Should().BeTrue();
        newToken.Should().NotBe(ccToken);
    }

    [Fact]
    public async Task Patch_Without_IfMatch_Returns_428()
    {
        var (client, _, _) = await CreateAuthedClientAsync();
        var dto = new PatchUserAccountDto { DisplayName = "X" };
        var resp = await PatchProfileAsync(client, null, dto);
        resp.StatusCode.Should().Be((HttpStatusCode)428);
    }

    [Fact]
    public async Task Patch_With_Old_Etag_Returns_412_And_Current_Etag()
    {
        var (client, userId, _) = await CreateAuthedClientAsync();
        var token1 = await GetCurrentConcurrencyTokenAsync(userId);
        var ifMatch1 = EtagUtils.ToEtag(token1);

        var resp1 = await PatchProfileAsync(client, ifMatch1, new PatchUserAccountDto { DisplayName = "A1" });
        resp1.EnsureSuccessStatusCode();

        // Reuse old ETag
        var resp2 = await PatchProfileAsync(client, ifMatch1, new PatchUserAccountDto { DisplayName = "A2" });
        resp2.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        resp2.Headers.ETag.Should().NotBeNull();
        EtagUtils.TryParseStringEtag(resp2.Headers.ETag!.Tag, out var current).Should().BeTrue();

        // Current should equal DB token now
        var dbToken = await GetCurrentConcurrencyTokenAsync(userId);
        current.Should().Be(dbToken);
    }

    [Fact]
    public async Task Patch_With_Wildcard_IfMatch_Succeeds()
    {
        var (client, _, _) = await CreateAuthedClientAsync();
        var resp = await PatchProfileAsync(client, "*", new PatchUserAccountDto { DisplayName = "Zed" });
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Patch_Email_Uniqueness_Conflict_Returns_409()
    {
        // user A
        var (clientA, userIdA, _) = await CreateAuthedClientAsync();
        var ccA = await GetCurrentConcurrencyTokenAsync(userIdA);
        var ifMatchA = EtagUtils.ToEtag(ccA);
        var email = $"{Guid.NewGuid():N}@example.com";
        var setA = await PatchProfileAsync(clientA, ifMatchA, new PatchUserAccountDto { Email = email });
        setA.EnsureSuccessStatusCode();

        // user B
        var (clientB, userIdB, _) = await CreateAuthedClientAsync();
        var ccB = await GetCurrentConcurrencyTokenAsync(userIdB);
        var ifMatchB = EtagUtils.ToEtag(ccB);
        var setB = await PatchProfileAsync(clientB, ifMatchB, new PatchUserAccountDto { Email = email });

        setB.StatusCode.Should().Be(HttpStatusCode.Conflict);
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

    private static async Task<HttpResponseMessage> PatchProfileAsync(HttpClient client, string? ifMatch, PatchUserAccountDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, "/v1/users/me")
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        if (!string.IsNullOrWhiteSpace(ifMatch))
            req.Headers.TryAddWithoutValidation("If-Match", ifMatch);

        return await client.SendAsync(req);
    }
}
