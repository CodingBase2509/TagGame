using TagGame.Shared.Domain.Games;
using TagGame.Shared.Domain.Players;

namespace TagGame.Shared.DTOs.Games;

public class PlayerLeftGameInfo
{
    public Player Player { get; set; }
    public PlayerDisconnectType DisconnectType { get; set; }
}