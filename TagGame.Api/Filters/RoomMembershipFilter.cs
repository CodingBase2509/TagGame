using System.Security.Claims;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Infrastructure.Auth;

namespace TagGame.Api.Filters;

public sealed class RoomMembershipFilter : IEndpointFilter
{
    private const string MembershipItemKey = "Membership";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var ct = http.RequestAborted;

        if (!AuthUtils.TryGetRoomId(http, out var roomId))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Bad request",
                detail: "Missing or invalid roomId in route.",
                extensions: new Dictionary<string, object?> { ["code"] = "room_id_missing" });
        }

        if (!AuthUtils.TryGetUserId(http, out var userId))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: "Invalid user identity.",
                extensions: new Dictionary<string, object?> { ["code"] = "auth.invalid_token" });
        }

        var gameUoW = http.RequestServices.GetRequiredService<IGamesUoW>();
        var membership = await AuthUtils.LoadMembershipAsync(gameUoW, userId, roomId, ct);

        if (membership is null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: "User is not a member of this room.",
                extensions: new Dictionary<string, object?> { ["code"] = "auth.not_member" });
        }

        if (membership.IsBanned)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: "User is banned in this room.",
                extensions: new Dictionary<string, object?> { ["code"] = "auth.banned" });
        }

        http.Items[MembershipItemKey] = membership;
        return await next(context);
    }
}
