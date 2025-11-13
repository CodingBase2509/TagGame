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
            case HttpContext http:
                await HandleHttpAsync(http, context, requirement);
                break;
            case HubInvocationContext hub:
                await HandleHubAsync(hub, context, requirement);
                break;
            default:
                return;
        }
    }

    private async Task HandleHttpAsync(HttpContext http, AuthorizationHandlerContext context, RoomPermissionRequirement requirement)
    {
        if (!AuthUtils.TryGetUserId(http, out var userId))
            return;
        if (!AuthUtils.TryGetRoomId(http, out var roomId))
            return;

        var membership = AuthUtils.TryGetMembershipFromItems(http)
                         ?? await AuthUtils.LoadMembershipAsync(gamesUoW, userId, roomId, http.RequestAborted);
        if (membership is null || membership.IsBanned)
            return;

        if (membership.PermissionsMask.Includes(requirement.Permission))
            context.Succeed(requirement);
    }

    private async Task HandleHubAsync(HubInvocationContext hub, AuthorizationHandlerContext context, RoomPermissionRequirement requirement)
    {
        if (!TryGetUserId(hub.Context.User!, out var userId))
            return;
        if (!TryGetRoomId(hub, out var roomId))
            return;

        var membership = TryGetMembershipFromItems(hub.Context)
                         ?? await AuthUtils.LoadMembershipAsync(gamesUoW, userId, roomId, hub.Context.ConnectionAborted);
        if (membership is null || membership.IsBanned)
            return;

        if (membership.PermissionsMask.Includes(requirement.Permission))
            context.Succeed(requirement);
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
            switch (arg)
            {
                case Guid gid:
                    roomId = gid;
                    return true;
                case string s when Guid.TryParse(s, out var parsed):
                    roomId = parsed;
                    return true;
                case null:
                    continue;
                default:
                    break;
            }

            var prop = arg.GetType().GetProperty(
                "RoomId",
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var val = prop?.GetValue(arg);
            switch (val)
            {
                case Guid guid:
                    roomId = guid;
                    return true;
                case string s2 when Guid.TryParse(s2, out var parsedFromString):
                    roomId = parsedFromString;
                    return true;
                default:
                    break;
            }
        }
        roomId = default;
        return false;
    }

    private static RoomMembership? TryGetMembershipFromItems(HubCallerContext ctx) =>
        ctx.Items.TryGetValue("Membership", out var value) ? value as RoomMembership : null;
}
