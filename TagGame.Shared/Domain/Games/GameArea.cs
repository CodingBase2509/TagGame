using TagGame.Shared.Domain.Common;

namespace TagGame.Shared.Domain.Games;

public class GameArea
{
    public GameAreaType Shape { get; set; }

    public Location[] Boundary { get; set; } = [];
}
