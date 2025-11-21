using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Carter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using TagGame.Api.Core.Features.Auth;
using TagGame.Api.Core.Persistence.Contexts;
using TagGame.Api.Tests.Integration.Games.TestModules;
using TagGame.Shared.Domain.Auth;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Tests.Integration.Games;

public sealed class AuthZPoliciesTests : IntegrationTestBase
{
    private WebApplicationFactory<Program>? _factory;

    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";
    private const string SigningKey = "integration-test-signing-key-0123456789";

    public override async Task InitializeAsync()
    {
        if (!DockerRequirement.IsAvailable)
            return;
        UseDbTestContainer();
        await base.InitializeAsync();

        // Dedicated DB for these tests
        if (_dbContainer is not null)
        {
            var cs = await CreateDatabaseAsync("api_games_authz");
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

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Register test Carter module with protected endpoints
                    services.AddSingleton<ICarterModule, AuthZTestModule>();
                });
            });

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
    public async Task Unfiltered_PermissionPolicy_Allows_When_Member_Has_Permission()
    {
        // Arrange
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        await SeedRoomAndMembershipAsync(roomId, ownerUserId: Guid.NewGuid(), userId, role: RoomRole.Player,
            mask: RoomPermission.StartGame | RoomPermission.EditSettings, banned: false);
        var client = CreateClientWithToken(userId);

        // Act
        var resp = await client.GetAsync($"/v1/_it/rooms/{roomId}/unfiltered/perm/start");
        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"[Unfiltered_PermissionPolicy_Allows] {resp.StatusCode} body={body}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerFact]
    public async Task Unfiltered_PermissionPolicy_Denies_When_Member_Lacks_Permission()
    {
        // Arrange
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        await SeedRoomAndMembershipAsync(roomId, ownerUserId: Guid.NewGuid(), userId, role: RoomRole.Player,
            mask: RoomPermission.Tag, banned: false);
        var client = CreateClientWithToken(userId);

        // Act
        var resp = await client.GetAsync($"/v1/_it/rooms/{roomId}/unfiltered/perm/start");
        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"[Unfiltered_PermissionPolicy_Denies] {resp.StatusCode} body={body}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task Unfiltered_RolePolicy_Allows_Owner_And_Denies_Moderator()
    {
        var roomId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var modId = Guid.NewGuid();
        await SeedRoomAndMembershipAsync(roomId, ownerUserId: ownerId, ownerId, role: RoomRole.Owner,
            mask: RoomPermission.ManageRoles | RoomPermission.StartGame, banned: false);
        await SeedMembershipOnlyAsync(roomId, modId, role: RoomRole.Moderator, mask: RoomPermission.StartGame, banned: false);

        var ownerClient = CreateClientWithToken(ownerId);
        var modClient = CreateClientWithToken(modId);

        var r1 = await ownerClient.GetAsync($"/v1/_it/rooms/{roomId}/unfiltered/role/owner");
        Console.WriteLine($"[Role Owner] {r1.StatusCode} {await r1.Content.ReadAsStringAsync()}");
        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        var r2 = await modClient.GetAsync($"/v1/_it/rooms/{roomId}/unfiltered/role/owner");
        Console.WriteLine($"[Role Owner (mod)] {r2.StatusCode} {await r2.Content.ReadAsStringAsync()}");
        r2.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task Filtered_Probe_403_When_NotMember()
    {
        // Arrange (room exists, but user not a member)
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await SeedRoomAsync(roomId, ownerUserId: Guid.NewGuid());
        var client = CreateClientWithToken(userId);

        // Act
        var resp = await client.GetAsync($"/v1/_it/rooms/{roomId}/filtered/probe");
        var dbg = await resp.Content.ReadAsStringAsync();
        Console.WriteLine($"[Filtered_NotMember] {resp.StatusCode} body={dbg}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task Filtered_Probe_403_When_Banned()
    {
        // Arrange
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        await SeedRoomAndMembershipAsync(roomId, ownerUserId: Guid.NewGuid(), userId, role: RoomRole.Player,
            mask: RoomPermission.Tag, banned: true);
        var client = CreateClientWithToken(userId);

        // Act
        var resp = await client.GetAsync($"/v1/_it/rooms/{roomId}/filtered/probe");
        Console.WriteLine($"[Filtered_Banned] {resp.StatusCode} body={await resp.Content.ReadAsStringAsync()}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be((int)HttpStatusCode.Forbidden);
    }

    [DockerFact]
    public async Task Filtered_PermissionPolicy_Allows_When_Member_Has_Permission()
    {
        // Arrange
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        await SeedRoomAndMembershipAsync(roomId, ownerUserId: Guid.NewGuid(), userId, role: RoomRole.Moderator,
            mask: RoomPermission.StartGame | RoomPermission.EditSettings, banned: false);
        var client = CreateClientWithToken(userId);

        // Act
        var resp = await client.GetAsync($"/v1/_it/rooms/{roomId}/filtered/perm/start");
        Console.WriteLine($"[Filtered_PermissionPolicy_Allows] {resp.StatusCode} body={await resp.Content.ReadAsStringAsync()}");

        // Assert
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
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

    private async Task SeedRoomAsync(Guid roomId, Guid ownerUserId)
    {
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var room = new GameRoom
        {
            Id = roomId,
            Name = $"Room-{roomId.ToString()[..8]}",
            AccessCode = roomId.ToString("N")[..6].ToUpperInvariant(),
            OwnerUserId = ownerUserId,
            CreatedAt = DateTimeOffset.UtcNow,
            Settings = new RoomSettings()
        };
        db.GameRooms.Add(room);
        await db.SaveChangesAsync();
    }

    private async Task SeedMembershipOnlyAsync(Guid roomId, Guid userId, RoomRole role, RoomPermission mask, bool banned)
    {
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GamesDbContext>();
        var membership = new RoomMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoomId = roomId,
            Role = role,
            PermissionsMask = mask,
            IsBanned = banned,
            JoinedAt = DateTimeOffset.UtcNow
        };
        db.Memberships.Add(membership);
        await db.SaveChangesAsync();
    }

    private async Task SeedRoomAndMembershipAsync(Guid roomId, Guid ownerUserId, Guid userId, RoomRole role, RoomPermission mask, bool banned)
    {
        await SeedRoomAsync(roomId, ownerUserId);
        await SeedMembershipOnlyAsync(roomId, userId, role, mask, banned);
    }
}
