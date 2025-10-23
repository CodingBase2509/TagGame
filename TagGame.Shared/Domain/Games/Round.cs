using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Shared.Domain.Games;

/// <summary>
/// Represents a single round within a match.
/// </summary>
public class Round
{
    /// <summary>Primary identifier of the round.</summary>
    public Guid Id { get; set; }

    /// <summary>Match this round belongs to.</summary>
    public Guid MatchId { get; set; }

    /// <summary>Sequential round number starting at 1.</summary>
    public int RoundNo { get; set; }

    /// <summary>Current phase of the round.</summary>
    public RoundPhase Phase { get; set; } = RoundPhase.Hide;

    /// <summary>UTC time when the round started.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>UTC time when the round ended (if any).</summary>
    public DateTimeOffset? EndedAt { get; set; }
}
