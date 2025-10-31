using TagGame.Shared.Domain.Games;

namespace TagGame.Api.Extensions;

public static class HttpContext_Membership
{
    public static RoomMembership? TryGetMembership(this HttpContext context)
    {
        return context.Items.TryGetValue(nameof(RoomMembership), out var membership)
            ? (RoomMembership)membership!
            : null;
    }
}
