namespace TagGame.Shared.Domain.Auth;

/// <summary>
/// Describes a subscription or entitlement for a user (e.g., Premium).
/// </summary>
public class Entitlement
{
    /// <summary>
    /// Primary identifier of the entitlement record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Owning user id.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Entitlement type (e.g., "Premium", "Plus").
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Source of the entitlement (e.g., "Apple", "Google", "Promo").
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// UTC time from when the entitlement is valid.
    /// </summary>
    public DateTimeOffset ValidFrom { get; set; }

    /// <summary>
    /// UTC time until when the entitlement is valid (inclusive); null if perpetual.
    /// </summary>
    public DateTimeOffset? ValidUntil { get; set; }

    /// <summary>
    /// Optional provider-specific payload (e.g., receipt JSON, purchase token).
    /// </summary>
    public string? ExternalPayload { get; set; }
}

