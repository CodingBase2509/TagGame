using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Core.Abstractions.Rooms;

public interface IRoomsService
{
    Task<GameRoom> CreateRoomAsync(Guid ownerUserId, string name, CancellationToken cancellationToken = default);

    Task<RoomMembership> CreateMembershipAsync(
        Guid userId,
        Guid roomId,
        RoomRole role,
        RoomPermission permissions,
        PlayerType type,
        CancellationToken cancellationToken = default);

    Task<GameRoom?> GetRoomByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default);

    Task<RoomMembership?> GetMembershipAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default);

    Task<bool> IsBannedAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default);

    Task<string> GenerateUniqueAccessCodeAsync(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
