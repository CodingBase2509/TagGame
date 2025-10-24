namespace TagGame.Shared.Domain.Games.Enums;

/// <summary>
/// Phase within a round.
/// </summary>
public enum RoundPhase
{
    /// <summary>Players are hiding.</summary>
    Hide = 0,
    /// <summary>Seekers are hunting.</summary>
    Hunt = 1,
    /// <summary>Round ended.</summary>
    End = 2
}

