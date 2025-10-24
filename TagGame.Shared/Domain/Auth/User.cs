namespace TagGame.Shared.Domain.Auth;

/// <summary>
/// Represents an application user (identity). Can start as anonymous and optionally be upgraded.
/// </summary>
public class User
{
    /// <summary>
    /// Primary identifier of the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Optional display name chosen by the user.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Optional e-mail if the account has been upgraded/bound.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Optional device identifier used for anonymous bootstrap.
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// Optional avatar color in hex (e.g., "#FFAA00").
    /// </summary>
    public string? AvatarColor { get; set; }

    /// <summary>
    /// Optional flags for global features (e.g., admin, beta).
    /// </summary>
    public int Flags { get; set; }

    /// <summary>
    /// UTC timestamp when the user was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp of the last observed activity.
    /// </summary>
    public DateTimeOffset? LastSeenAt { get; set; }
}

