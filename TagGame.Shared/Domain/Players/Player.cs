using TagGame.Shared.Domain.Common;
using TagGame.Shared.DTOs.Common;

namespace TagGame.Shared.Domain.Players;

public class Player : IIdentifiable
{
    public Guid Id { get; set; }

    public string? ConnectionId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public ColorDTO AvatarColor { get; set; }

    public PlayerType Type { get; set; } = PlayerType.Hider;

    public Location? Location { get; set; }
}
