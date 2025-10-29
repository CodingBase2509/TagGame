using Microsoft.AspNetCore.Authorization;
using TagGame.Api.Core.Common.Security;
using TagGame.Api.Infrastructure.Auth.Requirements;

namespace TagGame.Api.Infrastructure.Auth.Handler;

public sealed class RoomPermissionHandler(IGamesUoW gamesUoW) : AuthorizationHandler<RoomPermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomPermissionRequirement requirement)
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

        if (membership.PermissionsMask.Includes(requirement.Permission))
            context.Succeed(requirement);
    }
}
