using Microsoft.AspNetCore.Authorization;
using TagGame.Api.Infrastructure.Auth.Requirements;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Infrastructure.Auth.Handler;

public sealed class RoomRoleHandler(IGamesUoW gamesUoW) : AuthorizationHandler<RoomRoleRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomRoleRequirement requirement)
    {
        if (context.Resource is not HttpContext http)
            return;

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
    }

    private static bool RoleSatisfies(RoomRole actual, RoomRole required) =>
        actual == required
        || (actual == RoomRole.Owner && required is RoomRole.Moderator or RoomRole.Player)
        || (actual == RoomRole.Moderator && required == RoomRole.Player);
}
