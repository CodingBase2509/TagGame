using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Shared.Domain.Games;

/// <summary>
/// Membership of a user in a game room including role and permissions.
/// </summary>
public class RoomMembership
{
    /// <summary>
    /// Surrogate primary key for EF convenience (composite unique on UserId+RoomId recommended).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>Foreign key: user id.</summary>
    public Guid UserId { get; set; }

    /// <summary>Foreign key: room id.</summary>
    public Guid RoomId { get; set; }

    /// <summary>The role of the player in the game.</summary>
    public PlayerType Type { get; set; } = PlayerType.Hider;

    /// <summary>Role of the user within the room.</summary>
    public RoomRole Role { get; set; } = RoomRole.Player;

    /// <summary>Bitmask of effective permissions in the room.</summary>
    public RoomPermission PermissionsMask { get; set; } = RoomPermission.None;

    /// <summary>Whether the membership is banned.</summary>
    public bool IsBanned { get; set; }

    /// <summary>UTC time when the user joined the room.</summary>
    public DateTimeOffset JoinedAt { get; set; }
}
