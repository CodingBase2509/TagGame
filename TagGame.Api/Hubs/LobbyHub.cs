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
    public Task JoinRoom(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomMember)]
    public Task LeaveRoom(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.ManageRoles))]
    public Task UpdatePlayer(Guid roomId, Guid playerId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.EditSettings))]
    public Task UpdateSettings(Guid roomId) => throw new NotImplementedException();

    [Authorize(Policy = AuthPolicyPrefix.RoomPermission + nameof(RoomPermission.StartGame))]
    public Task StartGame(Guid roomId) => throw new NotImplementedException();
}
