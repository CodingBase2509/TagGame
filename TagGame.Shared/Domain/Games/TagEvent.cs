namespace TagGame.Shared.Domain.Games;

/// <summary>
/// Event emitted when a seeker tags another player during a round.
/// </summary>
public class TagEvent
{
    /// <summary>Primary identifier of the tag event.</summary>
    public Guid Id { get; set; }

    /// <summary>Round in which the tag occurred.</summary>
    public Guid RoundId { get; set; }

    /// <summary>User id of the seeker who tagged.</summary>
    public Guid TaggerUserId { get; set; }

    /// <summary>User id of the tagged player.</summary>
    public Guid TaggedUserId { get; set; }

    /// <summary>Distance in meters at the time of tagging.</summary>
    public double Distance { get; set; }

    /// <summary>UTC timestamp when the tag was registered.</summary>
    public DateTimeOffset Timestamp { get; set; }
}

