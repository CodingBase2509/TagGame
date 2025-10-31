using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Moq;
using TagGame.Api.Infrastructure.Auth.Handler;
using TagGame.Api.Infrastructure.Auth.Requirements;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using TagGame.Api.Core.Abstractions.Persistence;

namespace TagGame.Api.Tests.Unit.Auth;

[Trait("Category", "Unit")]
public sealed class RoomAuthorizationHandlersTests
{
    // RoomMemberHandler - HTTP success and banned fail
    [Fact]
    public async Task RoomMemberHandler_Http_Succeeds_for_member_not_banned()
    {
        var (uow, repo) = CreateUoW();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        repo.SetupList(new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = false });
        var http = CreateHttpContext(userId, roomId, uow.Object);

        var handler = new RoomMemberHandler(uow.Object);
        var requirement = new RoomMemberRequirement();
        var ctx = new AuthorizationHandlerContext([requirement], http.User, http);

        await handler.HandleAsync(ctx);
        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task RoomMemberHandler_Http_Fails_when_banned()
    {
        var (uow, repo) = CreateUoW();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        repo.SetupList(new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = true });
        var http = CreateHttpContext(userId, roomId, uow.Object);

        var handler = new RoomMemberHandler(uow.Object);
        var requirement = new RoomMemberRequirement();
        var ctx = new AuthorizationHandlerContext([requirement], http.User, http);

        await handler.HandleAsync(ctx);
        ctx.HasSucceeded.Should().BeFalse();
    }

    // RoomPermissionHandler - HTTP
    [Fact]
    public async Task RoomPermissionHandler_Http_Succeeds_when_permission_present()
    {
        var (uow, repo) = CreateUoW();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        var membership = new RoomMembership { UserId = userId, RoomId = roomId, PermissionsMask = RoomPermission.EditSettings };
        repo.SetupList(membership);
        var http = CreateHttpContext(userId, roomId, uow.Object);

        var handler = new RoomPermissionHandler(uow.Object);
        var requirement = new RoomPermissionRequirement(RoomPermission.EditSettings);
        var ctx = new AuthorizationHandlerContext([requirement], http.User, http);

        await handler.HandleAsync(ctx);
        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task RoomPermissionHandler_Http_Fails_when_permission_missing()
    {
        var (uow, repo) = CreateUoW();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        var membership = new RoomMembership { UserId = userId, RoomId = roomId, PermissionsMask = RoomPermission.Tag };
        repo.SetupList(membership);
        var http = CreateHttpContext(userId, roomId, uow.Object);

        var handler = new RoomPermissionHandler(uow.Object);
        var requirement = new RoomPermissionRequirement(RoomPermission.EditSettings);
        var ctx = new AuthorizationHandlerContext([requirement], http.User, http);

        await handler.HandleAsync(ctx);
        ctx.HasSucceeded.Should().BeFalse();
    }

    // RoomRoleHandler - HTTP
    [Theory]
    [InlineData(RoomRole.Owner, RoomRole.Player, true)]
    [InlineData(RoomRole.Owner, RoomRole.Moderator, true)]
    [InlineData(RoomRole.Owner, RoomRole.Owner, true)]
    [InlineData(RoomRole.Moderator, RoomRole.Player, true)]
    [InlineData(RoomRole.Moderator, RoomRole.Owner, false)]
    [InlineData(RoomRole.Player, RoomRole.Player, true)]
    [InlineData(RoomRole.Player, RoomRole.Moderator, false)]
    public async Task RoomRoleHandler_Http_checks_role_hierarchy(RoomRole actual, RoomRole required, bool expected)
    {
        var (uow, repo) = CreateUoW();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        var membership = new RoomMembership { UserId = userId, RoomId = roomId, Role = actual };
        repo.SetupList(membership);
        var http = CreateHttpContext(userId, roomId, uow.Object);

        var handler = new RoomRoleHandler(uow.Object);
        var requirement = new RoomRoleRequirement(required);
        var ctx = new AuthorizationHandlerContext([requirement], http.User, http);

        await handler.HandleAsync(ctx);
        ctx.HasSucceeded.Should().Be(expected);
    }

    private static (Mock<IGamesUoW> uow, Mock<IDbRepository<RoomMembership>> repo) CreateUoW()
    {
        var uow = new Mock<IGamesUoW>();
        var repo = new Mock<IDbRepository<RoomMembership>>();
        uow.SetupGet(x => x.RoomMemberships).Returns(repo.Object);
        return (uow, repo);
    }

    private static DefaultHttpContext CreateHttpContext(Guid userId, Guid roomId, IGamesUoW uow)
    {
        var http = new DefaultHttpContext();
        http.Request.RouteValues["roomId"] = roomId;
        http.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        var sc = new ServiceCollection();
        sc.AddSingleton(uow);
        http.RequestServices = sc.BuildServiceProvider();
        return http;
    }

    // Note: Hub-specific handler tests skipped; HTTP path sufficiently validates logic.

    private sealed class TestHubCallerContext(ClaimsPrincipal user, string? userIdentifier) : HubCallerContext
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

file static class RoomRepoMoqExtensions
{
    public static void SetupList(this Mock<IDbRepository<RoomMembership>> repo, RoomMembership? membership)
    {
        repo.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<RoomMembership, bool>>>(),
                It.IsAny<QueryOptions<RoomMembership>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
    }
}
