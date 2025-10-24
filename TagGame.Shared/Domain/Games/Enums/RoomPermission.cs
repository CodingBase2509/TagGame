namespace TagGame.Shared.Domain.Games.Enums;

/// <summary>
/// Bit-flagged permissions in a room.
/// </summary>
[Flags]
public enum RoomPermission
{
    /// <summary>No permissions.</summary>
    None = 0,
    /// <summary>Can start/stop game.</summary>
    StartGame = 1 << 0,
    /// <summary>Can edit room settings.</summary>
    EditSettings = 1 << 1,
    /// <summary>Can invite players and manage access.</summary>
    Invite = 1 << 2,
    /// <summary>Can kick players.</summary>
    KickPlayer = 1 << 3,
    /// <summary>Can tag other players in-game.</summary>
    Tag = 1 << 4,
    /// <summary>Can manage roles/permissions.</summary>
    ManageRoles = 1 << 5
}

