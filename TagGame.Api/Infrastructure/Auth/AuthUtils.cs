using System.Security.Claims;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Infrastructure.Auth;

public static class AuthUtils
{
    public static bool TryGetUserId(HttpContext http, out Guid userId)
    {
        var sub = http.User.FindFirstValue("sub") ??
                  http.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out userId);
    }

    public static bool TryGetRoomId(HttpContext http, out Guid roomId)
    {
        var rv = http.Request.RouteValues;
        return Try(rv, "roomId", out roomId) || Try(rv, "id", out roomId) || Try(rv, "room", out roomId);
    }

    private static bool Try(RouteValueDictionary rv, string key, out Guid id)
    {
        if (rv.TryGetValue(key, out var raw))
        {
            if (raw is Guid g || (raw is string s && Guid.TryParse(s, out g)))
            {
                id = g;
                return true;
            }
        }

        id = Guid.Empty;
        return false;
    }

    public static RoomMembership? TryGetMembershipFromItems(HttpContext http) =>
        http.Items.TryGetValue("Membership", out var value) ? value as RoomMembership : null;

    public static Task<RoomMembership?> LoadMembershipAsync(
        IGamesUoW uow,
        Guid userId,
        Guid roomId, CancellationToken ct) =>
        uow.RoomMemberships.FirstOrDefaultAsync(m => m.UserId == userId && m.RoomId == roomId,
            new QueryOptions<RoomMembership> { AsNoTracking = true }, ct);
}
