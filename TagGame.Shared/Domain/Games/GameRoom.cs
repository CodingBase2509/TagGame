using TagGame.Shared.Domain.Games.Enums;
using TagGame.Shared.Domain.Games.Geo;

namespace TagGame.Shared.Domain.Games;

/// <summary>
/// Aggregate root representing a game room.
/// </summary>
public class GameRoom
{
    /// <summary>Primary identifier of the room.</summary>
    public Guid Id { get; set; }

    /// <summary>Human-readable room name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short access code used to join a room.</summary>
    public string AccessCode { get; set; } = string.Empty;

    /// <summary>User id of the room owner.</summary>
    public Guid OwnerUserId { get; set; }

    /// <summary>Current state of the game in this room.</summary>
    public GameState State { get; set; } = GameState.Lobby;

    /// <summary>Configurable settings controlling the game behavior.</summary>
    public RoomSettings Settings { get; set; } = new();

    /// <summary>Optional boundary polygon limiting the game area.</summary>
    public GeoPolygon? Boundaries { get; set; }

    /// <summary>UTC time when the room was created.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
