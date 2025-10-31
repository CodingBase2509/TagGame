using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TagGame.Api.Infrastructure.Auth;
using TagGame.Shared.Contracts;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Hubs;

[Authorize]
public class LobbyHub : Hub<ILobbyClient>
{
    [Authorize(Policy = AuthPolicyPrefix.RoomMember)]
    Task JoinRoom(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomMember)]
    Task LeaveRoom(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.ManageRoles))]
    Task UpdatePlayer(Guid roomId, Guid playerId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.EditSettings))]
    Task UpdateSettings(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.StartGame))]
    Task StartGame(Guid roomId) => throw new NotImplementedException();
}
