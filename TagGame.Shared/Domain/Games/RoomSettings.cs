namespace TagGame.Shared.Domain.Games;

/// <summary>
/// Game room settings that define timings and gameplay parameters.
/// </summary>
public sealed class RoomSettings
{
    /// <summary>Hide phase duration in seconds.</summary>
    public int HideTimeSec { get; set; } = 60;

    /// <summary>Hunt phase duration in seconds.</summary>
    public int HuntTimeSec { get; set; } = 600;

    /// <summary>Maximum distance in meters to tag a player.</summary>
    public double TagRadiusM { get; set; } = 4;
}

