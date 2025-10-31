using System.Runtime.Serialization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using TagGame.Api.Filters;
using TagGame.Api.Hubs;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Shared.Domain.Games;
using Microsoft.AspNetCore.Http.Features;

namespace TagGame.Api.Tests.Unit.SignalR;

[Trait("Category", "Unit")]
public sealed class RoomAuthHubFilterTests
{
    [Fact]
    public async Task InvokeMethodAsync_WithoutAuthenticatedUser_throws_invalid_token()
    {
        // arrange
        var (filter, uow, _) = CreateFilter();
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var hub = new LobbyHub();
        var invocation = CreateInvocation(hub, nameof(LobbyHub.StartGame), [], user);

        // act
        var act = async () => await filter.InvokeMethodAsync(invocation, _ => ValueTask.FromResult<object?>(null));

        // assert
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("auth.invalid_token");
    }

    [Fact]
    public async Task InvokeMethodAsync_MissingRoomId_throws_room_id_missing()
    {
        // arrange
        var (filter, uow, _) = CreateFilter();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString())], "jwt"));
        var hub = new LobbyHub();
        var invocation = CreateInvocation(hub, nameof(LobbyHub.StartGame), [], user);

        // act
        var act = async () => await filter.InvokeMethodAsync(invocation, _ => ValueTask.FromResult<object?>(null));

        // assert
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("auth.room_id_missing");
    }

    [Fact]
    public async Task InvokeMethodAsync_NotMember_throws_not_member()
    {
        // arrange
        var (filter, uow, authz) = CreateFilter();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        SetupMembership(uow, membership: null);
        authz.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        var hub = new LobbyHub();
        var invocation = CreateInvocation(hub, nameof(LobbyHub.JoinRoom), [roomId], user);

        // act
        var act = async () => await filter.InvokeMethodAsync(invocation, _ => ValueTask.FromResult<object?>(null));

        // assert
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("auth.not_member");
    }

    [Fact]
    public async Task InvokeMethodAsync_Banned_throws_banned()
    {
        // arrange
        var (filter, uow, authz) = CreateFilter();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        SetupMembership(uow, new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = true });
        authz.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        var hub = new LobbyHub();
        var invocation = CreateInvocation(hub, nameof(LobbyHub.JoinRoom), [roomId], user);

        // act
        var act = async () => await filter.InvokeMethodAsync(invocation, _ => ValueTask.FromResult<object?>(null));

        // assert
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("auth.banned");
    }

    [Fact]
    public async Task InvokeMethodAsync_MissingPermission_throws_missing_permission()
    {
        // arrange
        var (filter, uow, authz) = CreateFilter();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        SetupMembership(uow, new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = false });
        authz.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Failed());

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        var hub = new LobbyHub();
        var invocation = CreateInvocation(hub, nameof(LobbyHub.StartGame), [roomId], user);

        // act
        var act = async () => await filter.InvokeMethodAsync(invocation, _ => ValueTask.FromResult<object?>(null));

        // assert
        await act.Should().ThrowAsync<HubException>()
            .WithMessage("auth.missing_permission");
    }

    [Fact]
    public async Task InvokeMethodAsync_Succeeds_calls_next_and_sets_items()
    {
        // arrange
        var (filter, uow, authz) = CreateFilter();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        var membership = new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = false };
        SetupMembership(uow, membership);
        authz.Setup(a => a.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object?>(), It.IsAny<IEnumerable<IAuthorizationRequirement>>()))
            .ReturnsAsync(AuthorizationResult.Success());

        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        var hub = new LobbyHub();
        var invocation = CreateInvocation(hub, nameof(LobbyHub.JoinRoom), [roomId], user);

        var called = false;

        // act
        var result = await filter.InvokeMethodAsync(invocation, _ =>
        {
            called = true;
            return ValueTask.FromResult<object?>("ok");
        });

        // assert
        called.Should().BeTrue();
        result.Should().Be("ok");
        invocation.Context.Items["Membership"].Should().BeSameAs(membership);
        invocation.Context.Items["RoomId"].Should().Be(roomId);
    }

    private static (RoomAuthHubFilter filter, Mock<IGamesUoW> uow, Mock<IAuthorizationService> authz) CreateFilter()
    {
        var uow = new Mock<IGamesUoW>();
        var repo = new Mock<IDbRepository<RoomMembership>>();
        uow.SetupGet(x => x.RoomMemberships).Returns(repo.Object);

        var provider = new Mock<IAuthorizationPolicyProvider>();
        provider.Setup(p => p.GetPolicyAsync(It.IsAny<string>())).ReturnsAsync(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        provider.Setup(p => p.GetDefaultPolicyAsync()).ReturnsAsync(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
        provider.Setup(p => p.GetFallbackPolicyAsync()).ReturnsAsync((AuthorizationPolicy?)null);

        var authz = new Mock<IAuthorizationService>();
        var logger = Mock.Of<ILogger<RoomAuthHubFilter>>();

        var filter = new RoomAuthHubFilter(uow.Object, provider.Object, authz.Object, logger);
        return (filter, uow, authz);
    }

    private static void SetupMembership(Mock<IGamesUoW> uow, RoomMembership? membership)
    {
        uow.Setup(x => x.RoomMemberships.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RoomMembership, bool>>>(), It.IsAny<QueryOptions<RoomMembership>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
    }

    private static HubInvocationContext CreateInvocation(Hub hub, string methodName, object[] args, ClaimsPrincipal user)
    {
        var caller = new TestHubCallerContext(user);

        var hic = (HubInvocationContext)FormatterServices.GetUninitializedObject(typeof(HubInvocationContext));
        TrySetBackingField(hic, "Context", caller);
        TrySetBackingField(hic, "Hub", hub);
        // HubMethodName may be computed; not required for our checks
        TrySetBackingField(hic, "HubMethodArguments", args);
        return hic;
    }

    private static void TrySetBackingField(object obj, string propertyName, object? value)
    {
        var field = obj.GetType()
                        .GetField($"<{propertyName}>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field is not null)
            field.SetValue(obj, value);
    }

    private sealed class TestHubCallerContext(ClaimsPrincipal user, string? userIdentifier = null) : HubCallerContext
    {
        public override string ConnectionId { get; } = Guid.NewGuid().ToString("N");

        public override string? UserIdentifier { get; } = userIdentifier;
        public override ClaimsPrincipal User => user;
        private readonly Dictionary<object, object?> _items = [];
        public override IDictionary<object, object?> Items => _items;
        public override CancellationToken ConnectionAborted { get; } = new(false);

        public override IFeatureCollection Features { get; } = new FeatureCollection();
        public override void Abort() { }
    }
}
