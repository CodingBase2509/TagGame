namespace TagGame.Shared.Domain.Games.Geo;

/// <summary>
/// Represents a geographic coordinate (latitude/longitude) in WGS84.
/// </summary>
public sealed class GeoPoint
{
    /// <summary>Latitude in degrees (−90..+90).</summary>
    public double Latitude { get; set; }

    /// <summary>Longitude in degrees (−180..+180).</summary>
    public double Longitude { get; set; }
}

