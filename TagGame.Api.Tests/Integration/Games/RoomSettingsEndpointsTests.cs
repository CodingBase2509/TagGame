using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using TagGame.Api.Core.Common.Http;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;
using TagGame.Shared.DTOs.Settings;

namespace TagGame.Api.Tests.Integration.Games;

public sealed class RoomSettingsEndpointsTests : IntegrationTestBase, IDisposable
{
    private WebApplicationFactory<Program>? _factory;

    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";
    private const string SigningKey = "integration-test-signing-key-room-settings-012345";

    public override async Task InitializeAsync()
    {
        if (!DockerRequirement.IsAvailable)
            return;

        UseDbTestContainer();
        await base.InitializeAsync();

        if (_dbContainer is not null)
        {
            var cs = await CreateDatabaseAsync("api_room_settings");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
            Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", cs);
        }

        Environment.SetEnvironmentVariable("Jwt__Issuer", Issuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", Audience);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", SigningKey);
        Environment.SetEnvironmentVariable("Jwt:Issuer", Issuer);
        Environment.SetEnvironmentVariable("Jwt:Audience", Audience);
        Environment.SetEnvironmentVariable("Jwt:SigningKey", SigningKey);

        _factory = new WebApplicationFactory<Program>();

        await using var scope = _factory.Services.CreateAsyncScope();
        var games = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        if ((await games.Database.GetPendingMigrationsAsync()).Any())
            await games.Database.MigrateAsync();
    }

    public override async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        _factory = null;
        await base.DisposeAsync();
    }

    public void Dispose()
    {
        _factory?.Dispose();
        _factory = null;
        GC.SuppressFinalize(this);
    }

    [DockerFact]
    public async Task GetSettings_Requires_Authentication()
    {
        var roomId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);

        var client = _factory!.CreateClient();
        var resp = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerFact]
    public async Task GetSettings_Returns_403_When_Not_Member()
    {
        var roomId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId, GameState.Lobby);

        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        var resp = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task GetSettings_Returns_403_When_Banned()
    {
        var roomId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId, GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Player, RoomPermission.Tag, banned: true);

        var client = CreateClientWithToken(userId);
        var resp = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task GetSettings_Returns_Etag_And_Respects_IfNoneMatch()
    {
        var roomId = Guid.NewGuid();
        var ownerUserId = Guid.NewGuid();
        var memberUserId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId, GameState.Lobby);
        await SeedMembershipAsync(roomId, memberUserId, RoomRole.Player, RoomPermission.Tag, banned: false);

        var client = CreateClientWithToken(memberUserId);
        var resp1 = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        resp1.StatusCode.Should().Be(HttpStatusCode.OK);
        resp1.Headers.ETag.Should().NotBeNull();

        EtagUtils.TryParseStringEtag(resp1.Headers.ETag!.Tag, out var token1).Should().BeTrue();
        token1.Should().BeGreaterThan(0u);

        var req2 = new HttpRequestMessage(HttpMethod.Get, $"/v1/rooms/{roomId}/settings");
        req2.Headers.TryAddWithoutValidation("If-None-Match", resp1.Headers.ETag!.Tag);
        var resp2 = await client.SendAsync(req2);
        resp2.StatusCode.Should().Be(HttpStatusCode.NotModified);
        resp2.Headers.ETag.Should().NotBeNull();
        resp2.Headers.ETag!.Tag.Should().Be(resp1.Headers.ETag!.Tag);

        var req3 = new HttpRequestMessage(HttpMethod.Get, $"/v1/rooms/{roomId}/settings");
        req3.Headers.TryAddWithoutValidation("If-None-Match", "W/\"abc\"");
        var resp3 = await client.SendAsync(req3);
        resp3.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerFact]
    public async Task PatchSettings_Requires_Authentication()
    {
        var roomId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);

        var client = _factory!.CreateClient();
        var resp = await PatchSettingsAsync(client, roomId, ifMatch: "*", new PatchRoomSettingsRequestDto { HideTimeSec = 120 });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerFact]
    public async Task PatchSettings_Without_IfMatch_Returns_428()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Owner, RoomPermission.EditSettings, banned: false);

        var client = CreateClientWithToken(userId);
        var resp = await PatchSettingsAsync(client, roomId, ifMatch: null, new PatchRoomSettingsRequestDto { HideTimeSec = 120 });
        resp.StatusCode.Should().Be((HttpStatusCode)428);
    }

    [DockerFact]
    public async Task PatchSettings_With_Empty_Patch_Returns_400()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Owner, RoomPermission.EditSettings, banned: false);

        var client = CreateClientWithToken(userId);
        var get = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        get.EnsureSuccessStatusCode();
        var etag = get.Headers.ETag!.Tag;

        var resp = await PatchSettingsAsync(client, roomId, etag, new PatchRoomSettingsRequestDto());
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerFact]
    public async Task PatchSettings_With_Mismatched_IfMatch_Returns_412_And_Current_Etag()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Owner, RoomPermission.EditSettings, banned: false);

        var client = CreateClientWithToken(userId);
        var get = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        get.EnsureSuccessStatusCode();
        var currentEtag = get.Headers.ETag!.Tag;
        EtagUtils.TryParseStringEtag(currentEtag, out var token).Should().BeTrue();

        var wrong = EtagUtils.ToEtag(token + 1u);
        var resp = await PatchSettingsAsync(client, roomId, wrong, new PatchRoomSettingsRequestDto { HideTimeSec = 121 });
        resp.StatusCode.Should().Be(HttpStatusCode.PreconditionFailed);
        resp.Headers.ETag.Should().NotBeNull();
        resp.Headers.ETag!.Tag.Should().Be(currentEtag);
    }

    [DockerFact]
    public async Task PatchSettings_Succeeds_Updates_Settings_And_Returns_New_Etag()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Owner, RoomPermission.EditSettings, banned: false);

        var client = CreateClientWithToken(userId);
        var get = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        get.EnsureSuccessStatusCode();
        var etag1 = get.Headers.ETag!.Tag;
        EtagUtils.TryParseStringEtag(etag1, out var token1).Should().BeTrue();

        var resp = await PatchSettingsAsync(client, roomId, etag1, new PatchRoomSettingsRequestDto { HideTimeSec = 180 });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Headers.ETag.Should().NotBeNull();
        EtagUtils.TryParseStringEtag(resp.Headers.ETag!.Tag, out var token2).Should().BeTrue();
        token2.Should().NotBe(token1);

        var body = await resp.Content.ReadFromJsonAsync<RoomSettingsResponseDto>();
        body.Should().NotBeNull();
        body!.HideTimeSec.Should().Be(180);

        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var room = await db.GameRooms.FindAsync(roomId);
        room.Should().NotBeNull();
        room!.Settings.HideTimeSec.Should().Be(180);
    }

    [DockerFact]
    public async Task PatchSettings_Returns_422_When_Room_Not_In_Lobby()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Preparation);
        await SeedMembershipAsync(roomId, userId, RoomRole.Owner, RoomPermission.EditSettings, banned: false);

        var client = CreateClientWithToken(userId);
        var get = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        get.EnsureSuccessStatusCode();
        var etag = get.Headers.ETag!.Tag;

        var resp = await PatchSettingsAsync(client, roomId, etag, new PatchRoomSettingsRequestDto { HideTimeSec = 200 });
        resp.StatusCode.Should().Be((HttpStatusCode)422);
        var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problem.Should().NotBeNull();
    }

    [DockerFact]
    public async Task PatchSettings_Returns_403_When_Missing_EditSettings_Permission()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Player, RoomPermission.Tag, banned: false);

        var client = CreateClientWithToken(userId);
        var get = await client.GetAsync($"/v1/rooms/{roomId}/settings");
        get.EnsureSuccessStatusCode();
        var etag = get.Headers.ETag!.Tag;

        var resp = await PatchSettingsAsync(client, roomId, etag, new PatchRoomSettingsRequestDto { HideTimeSec = 210 });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task PatchSettings_Returns_403_When_Banned()
    {
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid(), GameState.Lobby);
        await SeedMembershipAsync(roomId, userId, RoomRole.Owner, RoomPermission.EditSettings, banned: true);

        var client = CreateClientWithToken(userId);
        var resp = await PatchSettingsAsync(client, roomId, "*", new PatchRoomSettingsRequestDto { HideTimeSec = 210 });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private HttpClient CreateClientWithToken(Guid userId)
    {
        var client = _factory!.CreateClient();
        var token = CreateAccessToken(userId);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static string CreateAccessToken(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;
        var opts = new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            SigningKey = SigningKey,
            AccessMinutes = 30
        };
        var user = new User { Id = userId, DisplayName = "IT" };
        var (jwt, _) = AuthTokenHelper.CreateJwtToken(user, now, opts);
        return jwt;
    }

    private async Task SeedRoomAsync(Guid roomId, Guid ownerUserId, GameState state)
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var room = new GameRoom
        {
            Id = roomId,
            Name = $"Room-{roomId.ToString()[..8]}",
            AccessCode = roomId.ToString("N")[..8].ToUpperInvariant(),
            OwnerUserId = ownerUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            Settings = new RoomSettings(),
            State = state
        };
        db.GameRooms.Add(room);
        await db.SaveChangesAsync();
    }

    private async Task<Guid> SeedMembershipAsync(Guid roomId, Guid userId, RoomRole role, RoomPermission mask, bool banned)
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var membership = new RoomMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoomId = roomId,
            Role = role,
            PermissionsMask = mask,
            IsBanned = banned,
            JoinedAt = DateTimeOffset.UtcNow,
            Type = PlayerType.Hider
        };
        db.Memberships.Add(membership);
        await db.SaveChangesAsync();
        return membership.Id;
    }

    private static async Task<HttpResponseMessage> PatchSettingsAsync(
        HttpClient client,
        Guid roomId,
        string? ifMatch,
        PatchRoomSettingsRequestDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Patch, $"/v1/rooms/{roomId}/settings")
        {
            Content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, MediaTypeNames.Application.Json)
        };

        if (!string.IsNullOrWhiteSpace(ifMatch))
            req.Headers.TryAddWithoutValidation("If-Match", ifMatch);

        return await client.SendAsync(req);
    }
}
