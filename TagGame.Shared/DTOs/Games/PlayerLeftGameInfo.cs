using TagGame.Shared.Domain.Common;
using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Shared.DTOs.Games;

public class PlayerLeftGameInfo : IIdentifiable
{
    public Guid Id { get; set; }
    public Player Player { get; set; }
    public PlayerDisconnectType DisconnectType { get; set; }
}