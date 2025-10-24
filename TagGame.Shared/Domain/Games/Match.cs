using TagGame.Shared.Domain.Games.Enums;

namespace TagGame.Shared.Domain.Games;

/// <summary>
/// Represents a match within a room that may contain multiple rounds.
/// </summary>
public class Match
{
    /// <summary>Primary identifier of the match.</summary>
    public Guid Id { get; set; }

    /// <summary>Room this match belongs to.</summary>
    public Guid RoomId { get; set; }

    /// <summary>Current status of the match.</summary>
    public MatchStatus Status { get; set; } = MatchStatus.InProgress;

    /// <summary>Round number currently active (1-based).</summary>
    public int CurrentRoundNo { get; set; }

    /// <summary>UTC time when the match started.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>UTC time when the match ended (if completed/canceled).</summary>
    public DateTimeOffset? EndedAt { get; set; }
}
