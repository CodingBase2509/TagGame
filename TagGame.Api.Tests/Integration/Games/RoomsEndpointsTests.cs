using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Endpoints;
using TagGame.Api.Tests.Integration;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Shared.Domain.Auth;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;
using TagGame.Shared.DTOs.Rooms;

namespace TagGame.Api.Tests.Integration.Games;

public sealed class RoomsEndpointsTests : IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;

    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";
    private const string SigningKey = "integration-test-signing-key-rooms-012345";

    public override async Task InitializeAsync()
    {
        if (!DockerRequirement.IsAvailable)
            return;

        UseDbTestContainer();
        await base.InitializeAsync();

        // Dedicated DB for these tests
        if (_dbContainer is not null)
        {
            var cs = await CreateDatabaseAsync("api_rooms_endpoints");
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", cs);
            Environment.SetEnvironmentVariable("ConnectionStrings:DefaultConnection", cs);
        }

        // JWT configuration for the API
        Environment.SetEnvironmentVariable("Jwt__Issuer", Issuer);
        Environment.SetEnvironmentVariable("Jwt__Audience", Audience);
        Environment.SetEnvironmentVariable("Jwt__SigningKey", SigningKey);
        Environment.SetEnvironmentVariable("Jwt:Issuer", Issuer);
        Environment.SetEnvironmentVariable("Jwt:Audience", Audience);
        Environment.SetEnvironmentVariable("Jwt:SigningKey", SigningKey);

        _factory = new WebApplicationFactory<Program>();

        // Ensure GamesDbContext is migrated
        using var scope = _factory.Services.CreateScope();
        var games = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        if ((await games.Database.GetPendingMigrationsAsync()).Any())
            await games.Database.MigrateAsync();
    }

    public override async Task DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        await base.DisposeAsync();
    }

    [DockerFact]
    public async Task CreateRoom_Creates_room_and_owner_membership()
    {
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        var request = new CreateRoomRequestDto { Name = "My Room" };

        var resp = await client.PostAsJsonAsync("/v1/rooms", request);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var dto = await resp.Content.ReadFromJsonAsync<CreateRoomResponseDto>();
        dto.Should().NotBeNull();
        dto!.RoomId.Should().NotBe(Guid.Empty);
        dto.Name.Should().Be(request.Name);
        dto.MembershipId.Should().NotBe(Guid.Empty);

        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var room = await db.GameRooms.FindAsync(dto.RoomId);
        room.Should().NotBeNull();
        room!.OwnerUserId.Should().Be(userId);
        room.AccessCode.Should().MatchRegex("^[A-Za-z0-9]{8}$");

        var membership = await db.Memberships.FindAsync(dto.MembershipId);
        membership.Should().NotBeNull();
        membership!.UserId.Should().Be(userId);
        membership.Role.Should().Be(RoomRole.Owner);
    }

    [DockerFact]
    public async Task JoinRoom_creates_membership_when_not_member()
    {
        var roomOwnerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        const string accessCode = "ABCDEF12";
        await SeedRoomAsync(roomId, roomOwnerId, accessCode);

        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        var request = new JoinRoomRequestDto { AccessCode = accessCode };

        var resp = await client.PostAsJsonAsync("/v1/rooms/join", request);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await resp.Content.ReadFromJsonAsync<JoinRoomResponseDto>();
        dto.Should().NotBeNull();
        dto!.RoomId.Should().Be(roomId);
        dto.MembershipId.Should().NotBe(Guid.Empty);

        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var membership = await db.Memberships.FindAsync(dto.MembershipId);
        membership.Should().NotBeNull();
        membership!.UserId.Should().Be(userId);
        membership.IsBanned.Should().BeFalse();
    }

    [DockerFact]
    public async Task JoinRoom_returns_forbidden_when_banned()
    {
        var roomOwnerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        const string accessCode = "ABCDEFGH";
        var bannedUserId = Guid.NewGuid();
        await SeedRoomAsync(roomId, roomOwnerId, accessCode);
        await SeedMembershipAsync(roomId, bannedUserId, RoomRole.Player, RoomPermission.Tag, banned: true);

        var client = CreateClientWithToken(bannedUserId);
        var request = new JoinRoomRequestDto { AccessCode = accessCode };

        var resp = await client.PostAsJsonAsync("/v1/rooms/join", request);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task JoinRoom_returns_not_found_for_unknown_code()
    {
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        var request = new JoinRoomRequestDto { AccessCode = "ZZZZZZZZ" };

        var resp = await client.PostAsJsonAsync("/v1/rooms/join", request);

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be((int)HttpStatusCode.NotFound);
    }

    [DockerFact]
    public async Task JoinRoom_is_idempotent_for_existing_membership()
    {
        var roomOwnerId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        const string accessCode = "IDEMPOT1";
        var userId = Guid.NewGuid();

        await SeedRoomAsync(roomId, roomOwnerId, accessCode);
        var membershipId = await SeedMembershipAsync(roomId, userId, RoomRole.Player, RoomPermission.Tag, banned: false);

        var client = CreateClientWithToken(userId);
        var request = new JoinRoomRequestDto { AccessCode = accessCode };

        var resp = await client.PostAsJsonAsync("/v1/rooms/join", request);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await resp.Content.ReadFromJsonAsync<JoinRoomResponseDto>();
        dto!.MembershipId.Should().Be(membershipId);
    }

    [DockerFact]
    public async Task CreateRoom_requires_authentication()
    {
        var client = _factory!.CreateClient();
        var resp = await client.PostAsJsonAsync("/v1/rooms", new CreateRoomRequestDto { Name = "X" });

        var body = await resp.Content.ReadAsStringAsync();
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"body={body}");
    }

    [DockerFact]
    public async Task JoinRoom_requires_authentication()
    {
        var client = _factory!.CreateClient();
        var resp = await client.PostAsJsonAsync("/v1/rooms/join", new JoinRoomRequestDto { AccessCode = "ABCDEFGH" });

        var body = await resp.Content.ReadAsStringAsync();
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized, $"body={body}");
    }

    [DockerFact]
    public async Task CreateRoom_returns_bad_request_for_invalid_name()
    {
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        var resp = await client.PostAsJsonAsync("/v1/rooms", new CreateRoomRequestDto { Name = " " });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerFact]
    public async Task JoinRoom_returns_bad_request_for_invalid_access_code()
    {
        var userId = Guid.NewGuid();
        var client = CreateClientWithToken(userId);
        var resp = await client.PostAsJsonAsync("/v1/rooms/join", new JoinRoomRequestDto { AccessCode = " " });

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

    private async Task SeedRoomAsync(Guid roomId, Guid ownerUserId, string accessCode)
    {
        await using var scope = _factory!.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var room = new GameRoom
        {
            Id = roomId,
            Name = $"Room-{roomId.ToString()[..8]}",
            AccessCode = accessCode,
            OwnerUserId = ownerUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            Settings = new RoomSettings()
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
}
