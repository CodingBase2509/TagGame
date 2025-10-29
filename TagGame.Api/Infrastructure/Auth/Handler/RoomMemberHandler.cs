using Microsoft.AspNetCore.Authorization;
using TagGame.Api.Infrastructure.Auth.Requirements;

namespace TagGame.Api.Infrastructure.Auth.Handler;

public sealed class RoomMemberHandler(IGamesUoW gamesUoW) : AuthorizationHandler<RoomMemberRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, RoomMemberRequirement requirement)
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

        context.Succeed(requirement);
    }
}
