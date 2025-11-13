using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Infrastructure.Auth.Requirements;
using TagGame.Shared.Domain.Games.Enums;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Infrastructure.Auth.Handler;

public sealed class RoomRoleHandler(IGamesUoW gamesUoW) : AuthorizationHandler<RoomRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomRoleRequirement requirement)
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

                    if (RoleSatisfies(membership.Role, requirement.Role))
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

                    if (RoleSatisfies(membership.Role, requirement.Role))
                        context.Succeed(requirement);
                    break;
                }
        }
    }

    private static bool RoleSatisfies(RoomRole actual, RoomRole required) =>
        actual == required
        || (actual == RoomRole.Owner && required is RoomRole.Moderator or RoomRole.Player)
        || (actual == RoomRole.Moderator && required == RoomRole.Player);

    private static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
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
                    roomId = gid; return true;
                case string s when Guid.TryParse(s, out var gs):
                    roomId = gs; return true;
                case null:
                    continue;
            }

            var prop = arg.GetType().GetProperty("RoomId", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            var val = prop?.GetValue(arg);
            switch (val)
            {
                case Guid g2:
                    roomId = g2; return true;
                case string s2 when Guid.TryParse(s2, out var gs2):
                    roomId = gs2; return true;
            }
        }
        roomId = Guid.Empty;
        return false;
    }

    private static RoomMembership? TryGetMembershipFromItems(HubCallerContext ctx) =>
        ctx.Items.TryGetValue("Membership", out var value) ? value as RoomMembership : null;
}
