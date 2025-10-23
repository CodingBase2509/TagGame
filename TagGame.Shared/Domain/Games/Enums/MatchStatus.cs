namespace TagGame.Shared.Domain.Games.Enums;

/// <summary>
/// Match lifecycle state.
/// </summary>
public enum MatchStatus
{
    /// <summary>Match is in progress.</summary>
    InProgress = 0,
    /// <summary>Match finished successfully.</summary>
    Completed = 1,
    /// <summary>Match was canceled.</summary>
    Canceled = 2
}

