namespace TagGame.Shared.Domain.Games.Geo;

/// <summary>
/// Represents a polygon boundary (list of coordinates).
/// </summary>
public sealed class GeoPolygon
{
    /// <summary>
    /// Polygon coordinates ordered either clockwise or counter-clockwise.
    /// The first and last point do not need to be the same; consumers may close the ring.
    /// </summary>
    public List<GeoPoint> Points { get; set; } = [];
}

