using System.Security.Cryptography;
using TagGame.Api.Core.Abstractions.Persistence;
using TagGame.Api.Core.Abstractions.Rooms;
using TagGame.Api.Core.Common.Exceptions;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Api.Core.Features.Game;

public class RoomService(IGamesUoW uow, TimeProvider clock) : IRoomsService
{
    private const int AccessCodeLength = 8;
    private static readonly char[] AccessCodeAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public async Task<GameRoom> CreateRoomAsync(Guid ownerUserId, string name, CancellationToken cancellationToken = default)
    {
        var accessCode = await GenerateUniqueAccessCodeAsync(cancellationToken);

        var room = new GameRoom
        {
            Id = Guid.NewGuid(),
            CreatedAt = clock.GetUtcNow(),
            OwnerUserId = ownerUserId,
            Name = name,
            State = GameState.Lobby,
            Settings = new RoomSettings(),
            AccessCode = accessCode,
        };

        await uow.Rooms.AddAsync(room, cancellationToken);
        return room;
    }

    public async Task<GameRoom?> GetRoomByAccessCodeAsync(string accessCode, CancellationToken cancellationToken = default)
    {
        var room = await uow.Rooms
            .FirstOrDefaultAsync(r => Equals(r.AccessCode, accessCode), ct: cancellationToken);

        return room;
    }

    public async Task<RoomMembership> CreateMembershipAsync(
        Guid userId,
        Guid roomId,
        RoomRole role,
        RoomPermission permissions,
        PlayerType type,
        CancellationToken cancellationToken = default)
    {
        var membership = new RoomMembership
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoomId = roomId,
            Role = role,
            PermissionsMask = permissions,
            IsBanned = false,
            JoinedAt = clock.GetUtcNow(),
            Type = type
        };

        await uow.RoomMemberships.AddAsync(membership, cancellationToken);
        return membership;
    }

    public async Task<RoomMembership?> GetMembershipAsync(Guid userId, Guid roomId,
        CancellationToken cancellationToken = default)
    {
        var membership = await uow.RoomMemberships
            .FirstOrDefaultAsync(m => Equals(m.UserId, userId) && Equals(m.RoomId, roomId), ct: cancellationToken);

        return membership;
    }

    public async Task<bool> IsBannedAsync(Guid userId, Guid roomId, CancellationToken cancellationToken = default)
    {
        var membership = await GetMembershipAsync(userId, roomId, cancellationToken);

        return membership!.IsBanned;
    }

    public async Task<string> GenerateUniqueAccessCodeAsync(CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 20;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var code = GenerateCode();
            var existing = await uow.Rooms.FirstOrDefaultAsync(x => x.AccessCode == code, ct: cancellationToken);

            if (existing is null)
            {
                return code;
            }
        }

        throw new InvalidOperationException("Failed to generate unique access code without collisions.");
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await uow.SaveChangesAsync(cancellationToken);

    private static string GenerateCode()
    {
        Span<char> buffer = stackalloc char[AccessCodeLength];
        for (var i = 0; i < AccessCodeLength; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(AccessCodeAlphabet.Length);
            buffer[i] = AccessCodeAlphabet[idx];
        }

        return new string(buffer);
    }
}
