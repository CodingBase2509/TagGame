namespace TagGame.Shared.Domain.Games.Enums;

/// <summary>
/// Type of player in the game.
/// </summary>
public enum PlayerType
{
    /// <summary>Player is only watching the game.</summary>
    Spectator = 0,
    /// <summary>Player is a hider.</summary>
    Hider = 1,
    /// <summary>Player is a seeker.</summary>
    Seeker = 2
}
