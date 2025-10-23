namespace TagGame.Shared.Domain.Games.Enums;

/// <summary>
/// Current high-level state of a game room.
/// </summary>
public enum GameState
{
    /// <summary>Players are in the lobby and can join/ready.</summary>
    Lobby = 0,
    /// <summary>Countdown or preparation before hide phase.</summary>
    Preparation = 1,
    /// <summary>Players are hiding.</summary>
    Hide = 2,
    /// <summary>Seekers are hunting.</summary>
    Hunt = 3,
    /// <summary>Game or match finished, showing results.</summary>
    End = 4
}

