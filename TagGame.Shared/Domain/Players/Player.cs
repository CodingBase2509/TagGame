using System.Drawing;
using TagGame.Shared.Domain.Common;

namespace TagGame.Shared.Domain.Players;

public class Player
{
    public Guid Id { get; set; }

    public string? ConnectionId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public Color AvatarColor { get; set; }

    public PlayerType Type { get; set; } = PlayerType.Hider;

    public Location? Location { get; set; }
}
