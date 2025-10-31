using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TagGame.Api.Filters;
using TagGame.Shared.Domain.Games;
using TagGame.Api.Core.Abstractions.Persistence;

namespace TagGame.Api.Tests.Unit.Filters;

[Trait("Category", "Unit")]
public sealed class RoomMembershipFilterTests
{
    [Fact]
    public async Task Missing_roomId_returns_400_with_code()
    {
        var (filter, ctx, _) = CreateContext();

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        var (status, code) = await ExecuteResultAsync(result);
        status.Should().Be(StatusCodes.Status400BadRequest);
        code.Should().Be("room_id_missing");
    }

    [Fact]
    public async Task Invalid_user_returns_401_with_code()
    {
        var (filter, ctx, _) = CreateContext();
        ctx.HttpContext.Request.RouteValues["roomId"] = Guid.NewGuid();
        ctx.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        var (status, code) = await ExecuteResultAsync(result);
        status.Should().Be(StatusCodes.Status401Unauthorized);
        code.Should().Be("auth.invalid_token");
    }

    [Fact]
    public async Task Not_member_returns_403_with_code()
    {
        var (filter, ctx, uow) = CreateContext();
        ctx.HttpContext.Request.RouteValues["roomId"] = Guid.NewGuid();
        var userId = Guid.NewGuid();
        ctx.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        SetupMembership(uow, null);

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        var (status, code) = await ExecuteResultAsync(result);
        status.Should().Be(StatusCodes.Status403Forbidden);
        code.Should().Be("auth.not_member");
    }

    [Fact]
    public async Task Banned_returns_403_with_code()
    {
        var (filter, ctx, uow) = CreateContext();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        ctx.HttpContext.Request.RouteValues["roomId"] = roomId;
        ctx.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        SetupMembership(uow, new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = true });

        var result = await filter.InvokeAsync(ctx, _ => ValueTask.FromResult<object?>(null));

        var (status, code) = await ExecuteResultAsync(result);
        status.Should().Be(StatusCodes.Status403Forbidden);
        code.Should().Be("auth.banned");
    }

    [Fact]
    public async Task Valid_membership_calls_next_and_sets_item()
    {
        var (filter, ctx, uow) = CreateContext();
        var (userId, roomId) = (Guid.NewGuid(), Guid.NewGuid());
        ctx.HttpContext.Request.RouteValues["roomId"] = roomId;
        ctx.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", userId.ToString())], "jwt"));
        var membership = new RoomMembership { UserId = userId, RoomId = roomId, IsBanned = false };
        SetupMembership(uow, membership);

        var called = false;
        var result = await filter.InvokeAsync(ctx, _ =>
        {
            called = true;
            return ValueTask.FromResult<object?>("ok");
        });

        called.Should().BeTrue();
        result.Should().Be("ok");
        ctx.HttpContext.Items["Membership"].Should().BeSameAs(membership);
    }

    private static (RoomMembershipFilter filter, TestEndpointFilterInvocationContext ctx, Mock<IGamesUoW> uow) CreateContext()
    {
        var uow = new Mock<IGamesUoW>();
        var repo = new Mock<IDbRepository<RoomMembership>>();
        uow.SetupGet(x => x.RoomMemberships).Returns(repo.Object);

        var sc = new ServiceCollection();
        sc.AddSingleton(uow.Object);
        var sp = sc.BuildServiceProvider();

        var http = new DefaultHttpContext
        {
            RequestServices = sp,
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString())], "jwt"))
        };

        var ctx = new TestEndpointFilterInvocationContext(http);
        var filter = new RoomMembershipFilter();
        return (filter, ctx, uow);
    }

    private static void SetupMembership(Mock<IGamesUoW> uow, RoomMembership? membership)
    {
        uow.Setup(x => x.RoomMemberships.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<RoomMembership, bool>>>(), It.IsAny<QueryOptions<RoomMembership>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(membership);
    }

    private static Task<(int status, string? code)> ExecuteResultAsync(object? result)
    {
        result.Should().BeAssignableTo<object>();
        var status = (result as IStatusCodeHttpResult)?.StatusCode ?? 0;
        string? code = null;
        if (result is not IValueHttpResult { Value: ProblemDetails pd })
            return Task.FromResult((status, code));

        if (pd.Extensions.TryGetValue("code", out var v))
            code = v as string;
        return Task.FromResult((status, code));
    }

    private sealed class TestEndpointFilterInvocationContext(HttpContext http) : EndpointFilterInvocationContext
    {
        public override HttpContext HttpContext { get; } = http;
        public override IList<object?> Arguments { get; } = Array.Empty<object?>();
        public override T GetArgument<T>(int index) => (T)Arguments[index]!;
    }
}
