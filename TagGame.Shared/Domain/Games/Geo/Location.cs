namespace TagGame.Shared.Domain.Games.Geo;

/// <summary>
/// Represents a single location sample from a client, normalized on the server.
/// </summary>
public sealed class Location
{
    /// <summary>Latitude in degrees (−90..+90).</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude in degrees (−180..+180).</summary>
    public double Longitude { get; set; }

    /// <summary>Estimated accuracy in meters (1σ).</summary>
    public double Accuracy { get; set; }

    /// <summary>UTC timestamp of the measurement.</summary>
    public DateTimeOffset Timestamp { get; set; }
}

