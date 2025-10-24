namespace TagGame.Shared.Domain.Games.Enums;

/// <summary>
/// Role of a user inside a room.
/// </summary>
public enum RoomRole
{
    /// <summary>Room owner with full permissions.</summary>
    Owner = 0,
    /// <summary>Moderator with elevated permissions.</summary>
    Moderator = 1,
    /// <summary>Regular player.</summary>
    Player = 2
}

