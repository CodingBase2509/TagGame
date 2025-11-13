using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Core.Common.Security;
using TagGame.Api.Infrastructure.Auth.Requirements;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Infrastructure.Auth.Handler;

public sealed class RoomPermissionHandler(IGamesUoW gamesUoW) : AuthorizationHandler<RoomPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomPermissionRequirement requirement)
    {
        switch (context.Resource)
        {
            // HTTP path
            case HttpContext http:
            {
                if (!AuthUtils.TryGetUserId(http, out var userId))
                    return;
                if (!AuthUtils.TryGetRoomId(http, out var roomId))
                    return;

                var membership = AuthUtils.TryGetMembershipFromItems(http) ??
                                 await AuthUtils.LoadMembershipAsync(gamesUoW, userId, roomId, http.RequestAborted);
                if (membership is null || membership.IsBanned)
                    return;

                if (membership.PermissionsMask.Includes(requirement.Permission))
                    context.Succeed(requirement);
                return;
            }
            // Hub path
            case HubInvocationContext hub:
            {
                if (!TryGetUserId(hub.Context.User!, out var userId))
                    return;
                if (!TryGetRoomId(hub, out var roomId))
                    return;

                var membership = TryGetMembershipFromItems(hub.Context) ??
                                 await AuthUtils.LoadMembershipAsync(gamesUoW, userId, roomId, hub.Context.ConnectionAborted);
                if (membership is null || membership.IsBanned)
                    return;

                if (membership.PermissionsMask.Includes(requirement.Permission))
                    context.Succeed(requirement);
                break;
            }
        }
    }

    private static bool TryGetUserId(ClaimsPrincipal? user, out Guid userId)
    {
        ArgumentNullException.ThrowIfNull(user);
        var sub = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out userId);
    }

    private static bool TryGetRoomId(HubInvocationContext ctx, out Guid roomId)
    {
        if (ctx.Context.Items.TryGetValue("RoomId", out var value) && value is Guid g)
        {
            roomId = g;
            return true;
        }

        foreach (var arg in ctx.HubMethodArguments)
        {
            if (arg is Guid gid) { roomId = gid; return true; }
            if (arg is string s && Guid.TryParse(s, out var gs)) { roomId = gs; return true; }
            if (arg is null) continue;
            var prop = arg.GetType().GetProperty("RoomId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var val = prop?.GetValue(arg);
            if (val is Guid g2) { roomId = g2; return true; }
            if (val is string s2 && Guid.TryParse(s2, out var gs2)) { roomId = gs2; return true; }
        }
        roomId = default;
        return false;
    }

    private static RoomMembership? TryGetMembershipFromItems(HubCallerContext ctx) =>
        ctx.Items.TryGetValue("Membership", out var value) ? value as RoomMembership : null;
}
