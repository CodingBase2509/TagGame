using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Shared.Contracts;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Hubs;

[Authorize]
public class GameHub : Hub<IGameClient>
{
    [Authorize(Policy = AuthPolicyPrefix.RoomMember)]
    public Task UpdateLocation(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.Tag))]
    public Task TagPlayer(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomMember)]
    public Task SendChatMessage(Guid roomId) => throw new NotImplementedException();
}
