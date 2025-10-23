namespace TagGame.Shared.Domain.Auth;

/// <summary>
/// Represents a persisted refresh token for issuing new access tokens.
/// Tokens are rotated (one-time use) and tracked by a family id.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Primary identifier of the refresh token record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the owning user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Family identifier for token rotation tracking.
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// Hash of the refresh token (never store raw tokens).
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional reference to the token that replaced this one after rotation.
    /// </summary>
    public Guid? ReplacedById { get; set; }

    /// <summary>
    /// UTC time when the token was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// UTC time when the token expires and becomes invalid.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// UTC time when the token was revoked (e.g., on logout or reuse detection).
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }
}

